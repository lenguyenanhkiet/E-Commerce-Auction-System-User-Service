using ECommerceAuction.UserService.Domain.Auditing;

namespace ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.Common;

/// <summary>
/// A single audit log entry exposed to Admin.
/// </summary>
public sealed record AuditLogItem(
    Guid Id,
    Guid? ActorUserId,
    Guid? TargetUserId,
    string Action,
    string EntityType,
    string? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt)
{
    public static AuditLogItem FromEntity(UserAuditLog auditLog) => new(
        Id: auditLog.Id,
        ActorUserId: auditLog.ActorUserId,
        TargetUserId: auditLog.TargetUserId,
        Action: auditLog.Action,
        EntityType: auditLog.EntityType,
        EntityId: auditLog.EntityId,
        OldValue: auditLog.OldValue,
        NewValue: auditLog.NewValue,
        IpAddress: auditLog.IpAddress,
        UserAgent: auditLog.UserAgent,
        CreatedAt: auditLog.CreatedAt);
}

/// <summary>
/// Paginated audit log API response.
/// </summary>
public sealed record PagedAuditLogsResponse(
    IReadOnlyList<AuditLogItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
