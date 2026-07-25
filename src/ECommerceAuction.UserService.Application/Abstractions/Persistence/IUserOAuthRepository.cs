using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations required by Google OAuth2, refresh token, logout, role assignment, and audit logging flows.
/// </summary>
public interface IUserOAuthRepository
{
    /// <summary>
    /// Gets a user by internal user identifier.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user by normalized email address.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an external login link by provider name and provider user identifier.
    /// </summary>
    Task<UserExternalLogin?> GetExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new local user created or claimed by the OAuth2 flow.
    /// </summary>
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a provider login link between a local user and an external identity provider.
    /// </summary>
    Task AddExternalLoginAsync(UserExternalLogin externalLogin, CancellationToken cancellationToken);

    /// <summary>
    /// Gets active role codes assigned to a user.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetActiveRoleCodesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the distinct active privilege codes inherited from all active user roles.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetActivePrivilegeCodesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures that a user has the default BUYER role used by newly created OAuth2 users.
    /// </summary>
    Task EnsureDefaultBuyerRoleAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Ensures the user has a reputation profile and optionally grants the one-time email verification point.
    /// </summary>
    Task EnsureReputationProfileAsync(
        Guid userId,
        bool grantEmailVerificationPoint,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a refresh-token-backed user session.
    /// </summary>
    Task AddUserSessionAsync(UserSession session, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an active user session by hashed refresh token.
    /// </summary>
    Task<UserSession?> GetActiveSessionByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds an audit log entry for authentication-sensitive actions.
    /// </summary>
    Task AddAuditLogAsync(UserAuditLog auditLog, CancellationToken cancellationToken);



    
}
