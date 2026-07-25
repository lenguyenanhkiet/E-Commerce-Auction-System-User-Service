using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Users;

/// <summary>
/// Represents a login method linked from an external identity provider.
/// </summary>
public sealed class UserExternalLogin : AuditableEntity
{
    private UserExternalLogin()
    {
    }

    private UserExternalLogin(
        Guid userId,
        string provider,
        string providerUserId,
        string providerEmail,
        string? providerDisplayName)
    {
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        ProviderEmail = providerEmail;
        ProviderDisplayName = providerDisplayName;
        LinkedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
        Status = ExternalLoginStatuses.Active;
        CreatedAt = DateTime.UtcNow;
        // UserExternalLogins.updated_at is required by the database schema.
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ProviderUserId { get; private set; } = string.Empty;
    public string ProviderEmail { get; private set; } = string.Empty;
    public string? ProviderDisplayName { get; private set; }
    public string? AccessTokenHash { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTime LinkedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string Status { get; private set; } = ExternalLoginStatuses.Active;
    public User? User { get; private set; }

    /// <summary>
    /// Creates a Google external login record for the specified user.
    /// </summary>
    public static UserExternalLogin CreateGoogle(
        Guid userId,
        string providerUserId,
        string providerEmail,
        string? providerDisplayName)
    {
        return new UserExternalLogin(
            userId,
            AuthProviders.Google,
            providerUserId,
            providerEmail.Trim().ToLowerInvariant(),
            providerDisplayName?.Trim());
    }

    /// <summary>
    /// Checks whether this external login link can currently be used.
    /// </summary>
    public bool IsActive()
    {
        return Status == ExternalLoginStatuses.Active;
    }

    /// <summary>
    /// Updates mutable provider profile data while keeping the stable provider user id unchanged.
    /// </summary>
    public void SyncProviderSnapshot(string providerEmail, string? providerDisplayName)
    {
        // ECA-6 OAuth2 Google: provider_user_id is the stable identity; email/name are snapshots that can change over time.
        ProviderEmail = providerEmail.Trim().ToLowerInvariant();
        ProviderDisplayName = providerDisplayName?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the last login timestamp for this external login.
    /// </summary>
    public void MarkLoggedIn()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Defines lifecycle statuses for external login links.
/// </summary>
public static class ExternalLoginStatuses
{
    public const string Active = "ACTIVE";
    public const string Unlinked = "UNLINKED";
    public const string Revoked = "REVOKED";
}