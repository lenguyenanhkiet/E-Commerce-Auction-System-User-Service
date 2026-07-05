using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Abstractions.Persistence;

/// <summary>
/// Defines database operations required by Role Management without exposing EF Core to Application.
/// </summary>
public interface IRoleManagementRepository
{
    IQueryable<Role> QueryRoles();

    Task<IReadOnlyCollection<Role>> ListRolesAsync(
        IQueryable<Role> query,
        CancellationToken cancellationToken);

    Task<Role?> GetRoleByIdWithPrivilegesAsync(Guid roleId, CancellationToken cancellationToken);

    Task<bool> RoleCodeExistsAsync(
        string code,
        Guid? exceptRoleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Privilege>> GetActivePrivilegesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Privilege>> GetPrivilegesByCodesAsync(
        IReadOnlyCollection<string> privilegeCodes,
        CancellationToken cancellationToken);

    Task<bool> RoleHasActiveUsersAsync(Guid roleId, CancellationToken cancellationToken);

    Task AddRoleAsync(Role role, CancellationToken cancellationToken);

    Task AddRolePrivilegeAsync(RolePrivilege rolePrivilege, CancellationToken cancellationToken);

    void RemoveRolePrivilege(RolePrivilege rolePrivilege);

    Task AddAuditLogAsync(UserAuditLog auditLog, CancellationToken cancellationToken);
}
