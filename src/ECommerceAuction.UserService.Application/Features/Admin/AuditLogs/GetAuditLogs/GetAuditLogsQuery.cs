using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.Common;

namespace ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogs;

/// <summary>
/// Gets a filtered, paginated audit log list for Admin, most recent first.
/// </summary>
public sealed record GetAuditLogsQuery(
    string? Action = null,
    Guid? ActorUserId = null,
    Guid? TargetUserId = null,
    string? EntityType = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    int Page = 1,
    int PageSize = 20)
    : IQuery<PagedAuditLogsResponse>;
