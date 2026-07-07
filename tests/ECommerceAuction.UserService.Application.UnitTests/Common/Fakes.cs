using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Common;

/// <summary>
/// Shared test doubles for the Admin User Management handlers.
/// </summary>
internal static class Fakes
{
    /// <summary>
    /// Builds an <see cref="IRoleManagementRepository"/> stub backed by the four seeded roles.
    /// QueryRoles/ListRolesAsync execute the handler's LINQ in memory so filtering behaves like EF.
    /// </summary>
    public static (IRoleManagementRepository Repo, IReadOnlyDictionary<string, Role> ByCode) RoleRepository()
    {
        var roles = new List<Role>
        {
            Role.CreateSystemRole("ADMIN", "Administrator", null),
            Role.CreateSystemRole("BUYER", "Buyer", null),
            Role.CreateSystemRole("SELLER", "Seller", null),
            Role.CreateSystemRole("SUPPORT_STAFF", "Support Staff", null),
        };

        var repo = Substitute.For<IRoleManagementRepository>();
        repo.QueryRoles().Returns(roles.AsQueryable());
        repo.ListRolesAsync(Arg.Any<IQueryable<Role>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
                Task.FromResult<IReadOnlyCollection<Role>>(((IQueryable<Role>)callInfo[0]).ToList()));

        return (repo, roles.ToDictionary(role => role.Code, StringComparer.Ordinal));
    }

    /// <summary>
    /// Builds an <see cref="ICurrentUserService"/> that reports the given authenticated user id.
    /// </summary>
    public static ICurrentUserService CurrentUser(Guid userId)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(userId);
        currentUser.IsAuthenticated.Returns(true);
        return currentUser;
    }

    /// <summary>
    /// Builds a unit of work whose SaveChangesAsync reports one affected row.
    /// </summary>
    public static IUnitOfWork UnitOfWork()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        return unitOfWork;
    }
}
