using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Domain.Repositories;

/// <summary>
/// Read access to the immutable user audit log for Admin review.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets a filtered, paginated audit log page ordered by most recent first.
    /// </summary>
    Task<(IReadOnlyList<UserAuditLog> Items, int TotalCount)> GetPagedAsync(
        string? action,
        Guid? actorUserId,
        Guid? targetUserId,
        string? entityType,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single audit log entry by id.
    /// </summary>
    Task<UserAuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
