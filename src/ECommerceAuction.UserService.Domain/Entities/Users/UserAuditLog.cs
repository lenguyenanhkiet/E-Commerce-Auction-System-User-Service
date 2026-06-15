using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Records security-sensitive user actions for traceability and audit.
/// </summary>
public sealed class UserAuditLog : BaseEntity
{
    private UserAuditLog()
    {
    }

    private UserAuditLog(
        Guid? actorUserId,
        Guid? targetUserId,
        string action,
        string entityType,
        string? entityId,
        string? oldValue,
        string? newValue,
        string? ipAddress,
        string? userAgent)
    {
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValue = oldValue;
        NewValue = newValue;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid? ActorUserId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates an audit log entry for an authentication or account-management action.
    /// </summary>
    public static UserAuditLog Create(
        Guid? actorUserId,
        Guid? targetUserId,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserAuditLog(
            actorUserId,
            targetUserId,
            action,
            entityType,
            entityId,
            oldValue,
            newValue,
            ipAddress,
            userAgent);
    }
}

/// <summary>
/// Defines audit action names used by authentication flows.
/// </summary>
public static class UserAuditActions
{
    public const string LocalRegisterVerified = "LOCAL_REGISTER_VERIFIED";
    public const string LocalLogin = "LOCAL_LOGIN";
    public const string GoogleRegister = "GOOGLE_REGISTER";
    public const string GoogleLogin = "GOOGLE_LOGIN";
    public const string GoogleLink = "GOOGLE_LINK";
    public const string GoogleClaimUnverifiedAccount = "GOOGLE_CLAIM_UNVERIFIED_ACCOUNT";
    public const string GoogleEmailClaimConflict = "GOOGLE_EMAIL_CLAIM_CONFLICT";
    public const string RefreshTokenIssued = "REFRESH_TOKEN_ISSUED";
    public const string RefreshTokenRotated = "REFRESH_TOKEN_ROTATED";
    public const string Logout = "LOGOUT";
}
