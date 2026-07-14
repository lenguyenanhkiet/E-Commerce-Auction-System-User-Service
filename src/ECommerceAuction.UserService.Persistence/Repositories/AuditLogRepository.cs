using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

/// <summary>
/// EF Core read access to the user audit log.
/// </summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<UserAuditLog> Items, int TotalCount)> GetPagedAsync(
        string? action,
        Guid? actorUserId,
        Guid? targetUserId,
        string? entityType,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Audit logs are read-only, so tracking is unnecessary.
        IQueryable<UserAuditLog> query = _context.UserAuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim().ToUpperInvariant();
            query = query.Where(auditLog => auditLog.Action == normalizedAction);
        }

        if (actorUserId.HasValue)
        {
            query = query.Where(auditLog => auditLog.ActorUserId == actorUserId.Value);
        }

        if (targetUserId.HasValue)
        {
            query = query.Where(auditLog => auditLog.TargetUserId == targetUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalizedEntityType = entityType.Trim();
            query = query.Where(auditLog => auditLog.EntityType == normalizedEntityType);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(auditLog => auditLog.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(auditLog => auditLog.CreatedAt <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Most recent first; Id is the tie-breaker for stable pagination.
        var items = await query
            .OrderByDescending(auditLog => auditLog.CreatedAt)
            .ThenByDescending(auditLog => auditLog.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<UserAuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserAuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(auditLog => auditLog.Id == id, cancellationToken);
    }
}
