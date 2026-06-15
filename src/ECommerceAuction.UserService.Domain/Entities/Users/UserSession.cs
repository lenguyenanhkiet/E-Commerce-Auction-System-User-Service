using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Represents a refresh-token session for a user device or browser.
/// </summary>
public sealed class UserSession : BaseEntity
{
    private UserSession()
    {
    }

    private UserSession(
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        string? deviceId,
        string? deviceName,
        string? ipAddress,
        string? userAgent)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        DeviceId = deviceId;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string? DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new active refresh-token session.
    /// </summary>
    public static UserSession Create(
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        string? deviceId = null,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserSession(
            userId,
            refreshTokenHash,
            expiresAt,
            deviceId,
            deviceName,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Checks whether the refresh-token session can still be used.
    /// </summary>
    public bool IsActive()
    {
        return RevokedAt is null && ExpiresAt > DateTime.UtcNow;
    }

    /// <summary>
    /// Replaces the stored refresh token after a successful refresh operation.
    /// </summary>
    public void RotateRefreshToken(string refreshTokenHash, DateTime expiresAt)
    {
        // ECA-17 Refresh token: rotate token after refresh so an old refresh token cannot be reused forever.
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes the session so its refresh token can no longer be used.
    /// </summary>
    public void Revoke()
    {
        // ECA-10 Logout: revoke refresh session when FE sends the refresh token during logout.
        RevokedAt ??= DateTime.UtcNow;
        LastUsedAt = DateTime.UtcNow;
    }
}
