using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Users;

/// <summary>
/// EF Core bulk maintenance operations used by background jobs.
/// </summary>
public sealed class AccountMaintenanceRepository : IAccountMaintenanceRepository
{
    private readonly ApplicationDbContext _context;

    public AccountMaintenanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> UnlockExpiredLockoutsAsync(DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        // Set-based update: unlock every locked account whose lockout window has passed.
        return _context.Users
            .Where(user =>
                user.DeletedAt == null &&
                user.Status == UserStatus.Locked &&
                user.StatusExpiresAt != null &&
                user.StatusExpiresAt <= nowUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.Status, UserStatus.Active)
                .SetProperty(user => user.FailedLoginAttempts, 0)
                .SetProperty(user => user.StatusExpiresAt, (DateTimeOffset?)null)
                .SetProperty(user => user.UpdatedAt, nowUtc),
                cancellationToken);
    }

    public Task<int> FlagExpiredPasswordsAsync(DateTimeOffset passwordChangedBeforeUtc, CancellationToken cancellationToken = default)
    {
        // Only accounts that still have a local password can be forced to rotate it.
        return _context.Users
            .Where(user =>
                user.DeletedAt == null &&
                !user.MustChangePassword &&
                user.PasswordHash != string.Empty &&
                (user.PasswordChangedAt ?? user.CreatedAt) < passwordChangedBeforeUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.MustChangePassword, true)
                .SetProperty(user => user.UpdatedAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }
}
