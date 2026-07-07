using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.Common;

namespace ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogById;

/// <summary>
/// Gets the detail of a single audit log entry for Admin.
/// </summary>
public sealed record GetAuditLogByIdQuery(Guid AuditLogId) : IQuery<AuditLogItem>;
