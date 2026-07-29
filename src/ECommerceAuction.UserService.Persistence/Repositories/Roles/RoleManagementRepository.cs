using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Roles;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Roles;

/// <summary>
/// Implements Role Management persistence operations with EF Core.
/// </summary>
public sealed class RoleManagementRepository : IRoleManagementRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RoleManagementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Role?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .FirstOrDefaultAsync(
                role => role.Code == code,
                cancellationToken);
    }

    public IQueryable<Role> QueryRoles()
    {
        return _dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePrivileges)
            .ThenInclude(rolePrivilege => rolePrivilege.Privilege);
    }

    public async Task<IReadOnlyCollection<Role>> ListRolesAsync(
        IQueryable<Role> query,
        CancellationToken cancellationToken)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public Task<Role?> GetRoleByIdWithPrivilegesAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .Include(role => role.RolePrivileges)
            .ThenInclude(rolePrivilege => rolePrivilege.Privilege)
            .FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }

    public Task<bool> RoleCodeExistsAsync(
        string code,
        Guid? exceptRoleId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return _dbContext.Roles.AnyAsync(
            role =>
                role.Code == normalizedCode &&
                (!exceptRoleId.HasValue || role.Id != exceptRoleId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Privilege>> GetPrivilegesByCodesAsync(
        IReadOnlyCollection<string> privilegeCodes,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Privileges
            .Where(privilege =>
                privilege.Status == PrivilegeStatuses.Active &&
                privilegeCodes.Contains(privilege.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Privilege>> GetActivePrivilegesAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Privileges
            .AsNoTracking()
            .Where(privilege => privilege.Status == PrivilegeStatuses.Active)
            .OrderBy(privilege => privilege.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> RoleHasActiveUsersAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        // Only active, non-deleted users prevent role deletion.
        return _dbContext.UserRoles.AnyAsync(
            userRole =>
                userRole.RoleId == roleId &&
                userRole.Status == UserRoleStatuses.Active &&
                userRole.RevokedAt == null &&
                userRole.User.Status == UserStatus.Active &&
                userRole.User.DeletedAt == null,
            cancellationToken);
    }

    public Task AddRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return _dbContext.Roles.AddAsync(role, cancellationToken).AsTask();
    }

    public Task AddRolePrivilegeAsync(
        RolePrivilege rolePrivilege,
        CancellationToken cancellationToken)
    {
        return _dbContext.RolePrivileges.AddAsync(rolePrivilege, cancellationToken).AsTask();
    }

    public void RemoveRolePrivilege(RolePrivilege rolePrivilege)
    {
        _dbContext.RolePrivileges.Remove(rolePrivilege);
    }

    public Task AddAuditLogAsync(
        UserAuditLog auditLog,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserAuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
    }
}