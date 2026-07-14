using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogById;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Exposes read-only Admin access to the immutable user audit log.
/// </summary>
[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// GET /api/v1/admin/audit-logs — list audit logs, most recent first.
    ///
    /// Query parameters (all optional):
    /// action, actorUserId, targetUserId, entityType, fromUtc, toUtc, page (default 1), pageSize (default 20, max 100).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permissions.AuditLogs.View)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] Guid? actorUserId,
        [FromQuery] Guid? targetUserId,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(
            Action: action,
            ActorUserId: actorUserId,
            TargetUserId: targetUserId,
            EntityType: entityType,
            FromUtc: fromUtc,
            ToUtc: toUtc,
            Page: page,
            PageSize: pageSize);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(new
        {
            message = "Retrieved audit logs successfully.",
            data = result
        });
    }

    /// <summary>
    /// GET /api/v1/admin/audit-logs/{id} — view a single audit log entry.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.AuditLogs.View)]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAuditLogByIdQuery(id), cancellationToken);

        return Ok(new
        {
            message = "Retrieved audit log successfully.",
            data = result
        });
    }
}
