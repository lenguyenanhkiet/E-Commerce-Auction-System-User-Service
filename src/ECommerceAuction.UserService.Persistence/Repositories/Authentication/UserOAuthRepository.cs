using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Authentication;

/// <summary>
/// Provides User Service persistence operations required by OAuth2 authentication flows.
/// </summary>
public sealed class UserOAuthRepository : IUserOAuthRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserOAuthRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds a user by the internal application user id.
    /// </summary>
    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Finds a user by normalized email.
    /// </summary>
    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Finds an external login by provider and stable provider user id.
    /// </summary>
    public Task<UserExternalLogin?> GetExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserExternalLogins
            .Include(externalLogin => externalLogin.User)
            .FirstOrDefaultAsync(
                externalLogin => externalLogin.Provider == provider &&
                                 externalLogin.ProviderUserId == providerUserId,
                cancellationToken);
    }

    /// <summary>
    /// Adds a new user to the current unit of work.
    /// </summary>
    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    /// <summary>
    /// Adds a new external login link to the current unit of work.
    /// </summary>
    public Task AddExternalLoginAsync(
        UserExternalLogin externalLogin,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserExternalLogins.AddAsync(externalLogin, cancellationToken).AsTask();
    }

    /// <summary>
    /// Gets active role codes assigned to a user.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetActiveRoleCodesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserRoles
            .Where(userRole =>
                userRole.UserId == userId &&
                userRole.Status == UserRoleStatuses.Active &&
                userRole.RevokedAt == null)
            .Join(
                _dbContext.Roles.Where(role => role.Status == RoleStatuses.Active),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads effective privileges through active UserRole and RolePrivilege rows.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetActivePrivilegeCodesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserRoles
            .Where(userRole =>
                userRole.UserId == userId &&
                userRole.Status == UserRoleStatuses.Active &&
                userRole.RevokedAt == null)
            .Join(
                _dbContext.Roles.Where(role => role.Status == RoleStatuses.Active),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Id)
            .Join(
                _dbContext.RolePrivileges,
                roleId => roleId,
                rolePrivilege => rolePrivilege.RoleId,
                (_, rolePrivilege) => rolePrivilege.PrivilegeId)
            .Join(
                _dbContext.Privileges.Where(
                    privilege => privilege.Status == PrivilegeStatuses.Active),
                privilegeId => privilegeId,
                privilege => privilege.Id,
                (_, privilege) => privilege.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures a user has the default BUYER role required by the MVP authentication flow.
    /// </summary>
    public async Task EnsureDefaultBuyerRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // New OAuth users must receive the default BUYER role instead of using hard-coded roles.
        var buyerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(role => role.Code == RoleCodes.Buyer, cancellationToken);

        if (buyerRole is null)
        {
            buyerRole = Role.CreateSystemRole(
                RoleCodes.Buyer,
                "Buyer",
                "Default role for users who can buy products and join auctions.");

            await _dbContext.Roles.AddAsync(buyerRole, cancellationToken);
        }

        var alreadyAssigned = await _dbContext.UserRoles
            .AnyAsync(
                userRole =>
                    userRole.UserId == userId &&
                    userRole.RoleId == buyerRole.Id &&
                    userRole.Status == UserRoleStatuses.Active &&
                    userRole.RevokedAt == null,
                cancellationToken);

        if (!alreadyAssigned)
        {
            await _dbContext.UserRoles.AddAsync(
                UserRole.Assign(userId, buyerRole.Id),
                cancellationToken);
        }
    }

    /// <summary>
    /// Adds a refresh-token session to the current unit of work.
    /// </summary>
    public Task AddUserSessionAsync(
        UserSession session,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserSessions.AddAsync(session, cancellationToken).AsTask();
    }

    /// <summary>
    /// Finds an active user session by refresh-token hash.
    /// </summary>
    public Task<UserSession?> GetActiveSessionByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserSessions
            .FirstOrDefaultAsync(
                session =>
                    session.RefreshTokenHash == refreshTokenHash &&
                    session.RevokedAt == null &&
                    session.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);
    }

    /// <summary>
    /// Adds an audit log entry to the current unit of work.
    /// </summary>
    public Task AddAuditLogAsync(
        UserAuditLog auditLog,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserAuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
    }
}
