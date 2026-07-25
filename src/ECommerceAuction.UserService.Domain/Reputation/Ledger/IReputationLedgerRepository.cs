using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

public interface IReputationLedgerRepository
{
    Task<ReputationLedgerEntry?> GetByIdAsync(
        Guid entryId,
        CancellationToken cancellationToken = default);

    Task<ReputationLedgerEntry?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReputationLedgerEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReputationLedgerEntry>>
        GetConfirmablePendingEntriesAsync(
            DateTime asOf,
            int batchSize,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        ReputationLedgerEntry entry,
        CancellationToken cancellationToken = default);
}