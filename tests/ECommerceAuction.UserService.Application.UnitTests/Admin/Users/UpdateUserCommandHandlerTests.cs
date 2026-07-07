using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.Users.UpdateUser;
using ECommerceAuction.UserService.Application.UnitTests.Common;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Admin.Users;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserService _currentUser = Fakes.CurrentUser(Guid.NewGuid());
    private readonly IUnitOfWork _unitOfWork = Fakes.UnitOfWork();
    private readonly IRoleManagementRepository _roleRepository;
    private readonly IReadOnlyDictionary<string, Role> _rolesByCode;
    private readonly List<UserAuditLog> _audits = new();

    public UpdateUserCommandHandlerTests()
    {
        (_roleRepository, _rolesByCode) = Fakes.RoleRepository();
        _userRepository.CheckEmailExistsForOtherUserAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _userRepository.AddAuditLogAsync(
            Arg.Do<UserAuditLog>(log => _audits.Add(log)), Arg.Any<CancellationToken>());
    }

    private UpdateUserCommandHandler CreateSut() =>
        new(_userRepository, _roleRepository, _currentUser, _unitOfWork);

    private User CreateUserWithRoles(params string[] roleCodes)
    {
        var user = User.CreateByAdmin("target@test.local", "HASH", "Target", "0900000000", null, null, null);
        foreach (var code in roleCodes)
        {
            user.AssignRole(UserRole.Assign(user.Id, _rolesByCode[code].Id, Guid.NewGuid()));
        }

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    private static UpdateUserCommand Command(Guid id, IReadOnlyList<string>? roleCodes) =>
        new(id, "Updated Name", Email: null, Gender: "Male",
            DateOfBirth: new DateOnly(1990, 1, 1), Address: "Hue", RoleCodes: roleCodes);

    [Fact]
    public async Task Handle_UpdatesFields_AndLeavesRolesUnchanged_WhenRoleCodesNull()
    {
        var user = CreateUserWithRoles("BUYER");

        var result = await CreateSut().Handle(Command(user.Id, roleCodes: null), CancellationToken.None);

        Assert.Equal("Updated Name", user.FullName);
        Assert.Equal(new[] { "BUYER" }, result.Roles);
        Assert.Single(user.UserRoles); // no revoke, no add
        Assert.DoesNotContain(_audits, log => log.Action == UserAuditActions.UserRoleAssigned);
        Assert.Contains(_audits, log => log.Action == UserAuditActions.UserUpdated);
    }

    [Fact]
    public async Task Handle_ReconcilesRoles_RevokingRemoved_AndAddingNew()
    {
        var user = CreateUserWithRoles("BUYER");

        var result = await CreateSut().Handle(
            Command(user.Id, new[] { "SELLER" }), CancellationToken.None);

        Assert.Equal(new[] { "SELLER" }, result.Roles);

        var buyer = user.UserRoles.Single(r => r.RoleId == _rolesByCode["BUYER"].Id);
        var seller = user.UserRoles.Single(r => r.RoleId == _rolesByCode["SELLER"].Id);
        Assert.Equal(UserRoleStatuses.Revoked, buyer.Status);
        Assert.NotNull(buyer.RevokedAt);
        Assert.Equal(UserRoleStatuses.Active, seller.Status);
        Assert.Contains(_audits, log => log.Action == UserAuditActions.UserRoleAssigned);
    }

    [Fact]
    public async Task Handle_AddsRole_KeepingExisting_WhenSupersetRequested()
    {
        var user = CreateUserWithRoles("SELLER");

        var result = await CreateSut().Handle(
            Command(user.Id, new[] { "seller", "SUPPORT_STAFF" }), CancellationToken.None);

        Assert.Equal(new[] { "SELLER", "SUPPORT_STAFF" }, result.Roles);
        var activeCount = user.UserRoles.Count(r => r.RevokedAt is null);
        Assert.Equal(2, activeCount);
    }

    [Fact]
    public async Task Handle_RevokesAllRoles_WhenEmptyListProvided()
    {
        var user = CreateUserWithRoles("BUYER", "SELLER");

        var result = await CreateSut().Handle(
            Command(user.Id, Array.Empty<string>()), CancellationToken.None);

        Assert.Empty(result.Roles);
        Assert.All(user.UserRoles, role => Assert.Equal(UserRoleStatuses.Revoked, role.Status));
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotFound()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().Handle(Command(id, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenRoleCodeInvalid()
    {
        var user = CreateUserWithRoles("BUYER");

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().Handle(Command(user.Id, new[] { "GHOST" }), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenNewEmailBelongsToAnotherUser()
    {
        var user = CreateUserWithRoles("BUYER");
        _userRepository.CheckEmailExistsForOtherUserAsync(
            "taken@test.local", user.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateUserCommand(
            user.Id, "Updated Name", Email: "taken@test.local",
            Gender: null, DateOfBirth: null, Address: null);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ChangesEmail_WhenNewEmailIsFree()
    {
        var user = CreateUserWithRoles("BUYER");

        var command = new UpdateUserCommand(
            user.Id, "Updated Name", Email: "fresh@test.local",
            Gender: null, DateOfBirth: null, Address: null);

        await CreateSut().Handle(command, CancellationToken.None);

        Assert.Equal("fresh@test.local", user.Email);
    }
}
