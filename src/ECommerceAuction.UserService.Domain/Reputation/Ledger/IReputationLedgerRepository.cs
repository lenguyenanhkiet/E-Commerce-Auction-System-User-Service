namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

public interface IReputationLedgerRepository
{
    Task<ReputationLedgerEntry?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ReputationLedgerEntry?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<ReputationLedgerEntry?> GetBySourceAsync(
        string sourceId,
        string reasonCode,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<bool> HasReversalAsync(
        Guid originalEntryId,
        CancellationToken cancellationToken = default);

    Task<bool> HasReasonAsync(
        Guid userId,
        string role,
        string reasonCode,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ReputationLedgerEntry> Items, int Total)> GetPagedAsync(
        Guid userId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ReputationLedgerEntry entry,
        CancellationToken cancellationToken = default);
}
