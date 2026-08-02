using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Reputation;

public sealed class ReputationLedgerRepository : IReputationLedgerRepository
{
    private readonly ApplicationDbContext _context;

    public ReputationLedgerRepository(ApplicationDbContext context) =>
        _context = context;

    public Task<ReputationLedgerEntry?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.FirstOrDefaultAsync(
            entry => entry.Id == id,
            cancellationToken);

    public Task<ReputationLedgerEntry?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.FirstOrDefaultAsync(
            entry => entry.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public Task<ReputationLedgerEntry?> GetBySourceAsync(
        string sourceId,
        string reasonCode,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.FirstOrDefaultAsync(
            entry => entry.SourceId == sourceId && entry.ReasonCode == reasonCode,
            cancellationToken);

    public Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.AnyAsync(
            entry => entry.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public Task<bool> HasReversalAsync(
        Guid originalEntryId,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.AnyAsync(
            entry => entry.ReversesEntryId == originalEntryId,
            cancellationToken);

    public Task<bool> HasReasonAsync(
        Guid userId,
        string role,
        string reasonCode,
        CancellationToken cancellationToken = default) =>
        _context.ReputationLedgerEntries.AnyAsync(
            entry =>
                entry.UserId == userId &&
                entry.Role == role &&
                entry.ReasonCode == reasonCode,
            cancellationToken);

    public async Task<(IReadOnlyList<ReputationLedgerEntry> Items, int Total)> GetPagedAsync(
        Guid userId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ReputationLedgerEntries.AsNoTracking()
            .Where(entry => entry.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(
        ReputationLedgerEntry entry,
        CancellationToken cancellationToken = default) =>
        await _context.ReputationLedgerEntries.AddAsync(
            entry,
            cancellationToken);
}
