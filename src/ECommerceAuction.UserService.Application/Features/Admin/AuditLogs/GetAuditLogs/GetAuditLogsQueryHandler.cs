using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.Common;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogs;

/// <summary>
/// Handles Admin audit log list queries.
/// </summary>
public sealed class GetAuditLogsQueryHandler
    : IQueryHandler<GetAuditLogsQuery, PagedAuditLogsResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedAuditLogsResponse> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new BusinessRuleException("Page must be greater than or equal to 1.");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            throw new BusinessRuleException("PageSize must be between 1 and 100.");
        }

        if (request.FromUtc.HasValue && request.ToUtc.HasValue && request.FromUtc > request.ToUtc)
        {
            throw new BusinessRuleException("FromUtc must be earlier than or equal to ToUtc.");
        }

        var result = await _auditLogRepository.GetPagedAsync(
            action: request.Action,
            actorUserId: request.ActorUserId,
            targetUserId: request.TargetUserId,
            entityType: request.EntityType,
            fromUtc: request.FromUtc,
            toUtc: request.ToUtc,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var items = result.Items.Select(AuditLogItem.FromEntity).ToList();

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)request.PageSize);

        return new PagedAuditLogsResponse(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: totalPages);
    }
}
