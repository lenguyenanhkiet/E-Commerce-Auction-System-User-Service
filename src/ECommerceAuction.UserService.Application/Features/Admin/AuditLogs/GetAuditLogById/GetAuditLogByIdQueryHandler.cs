using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.Common;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Admin.AuditLogs.GetAuditLogById;

/// <summary>
/// Handles Admin single audit log detail queries.
/// </summary>
public sealed class GetAuditLogByIdQueryHandler
    : IQueryHandler<GetAuditLogByIdQuery, AuditLogItem>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogByIdQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<AuditLogItem> Handle(
        GetAuditLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        var auditLog = await _auditLogRepository.GetByIdAsync(request.AuditLogId, cancellationToken)
            ?? throw new NotFoundException($"Audit log {request.AuditLogId} was not found.");

        return AuditLogItem.FromEntity(auditLog);
    }
}
