using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

public sealed class ReputationLedgerRepository : IReputationLedgerRepository
{
    private readonly ApplicationDbContext _context;

    public ReputationLedgerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ReputationLedgerEntry?> GetByIdAsync(
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        return _context.ReputationLedgerEntries
            .FirstOrDefaultAsync(entry => entry.Id == entryId, cancellationToken);
    }

    public Task<ReputationLedgerEntry?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return _context.ReputationLedgerEntries
            .FirstOrDefaultAsync(entry => entry.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return _context.ReputationLedgerEntries
            .AnyAsync(entry => entry.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<IReadOnlyList<ReputationLedgerEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReputationLedgerEntries
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReputationLedgerEntry>> GetConfirmablePendingEntriesAsync(
        DateTimeOffset asOf,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReputationLedgerEntries
            .Where(entry =>
                entry.Status == ReputationEntryStatuses.Pending &&
                entry.ConfirmAfter != null &&
                entry.ConfirmAfter <= asOf)
            .OrderBy(entry => entry.ConfirmAfter)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ReputationLedgerEntry entry,
        CancellationToken cancellationToken = default)
    {
        await _context.ReputationLedgerEntries.AddAsync(entry, cancellationToken);
    }
}
