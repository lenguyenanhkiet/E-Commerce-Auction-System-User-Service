using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

public class UserPasswordHistoryRepository : IUserPasswordHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public UserPasswordHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetRecentHashesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserPasswordHistories
            .AsNoTracking()
            .Where(history => history.UserId == userId)
            .OrderByDescending(history => history.CreatedAt)
            .Take(count)
            .Select(history => history.PasswordHash)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        UserPasswordHistory entry,
        CancellationToken cancellationToken = default)
    {
        await _context.UserPasswordHistories.AddAsync(entry, cancellationToken);
    }

    public async Task PruneOldEntriesAsync(
        Guid userId,
        int keep,
        CancellationToken cancellationToken = default)
    {
        // Keep the newest `keep` entries and remove anything older.
        var staleEntries = await _context.UserPasswordHistories
            .Where(history => history.UserId == userId)
            .OrderByDescending(history => history.CreatedAt)
            .Skip(keep)
            .ToListAsync(cancellationToken);

        if (staleEntries.Count > 0)
        {
            _context.UserPasswordHistories.RemoveRange(staleEntries);
        }
    }
}
