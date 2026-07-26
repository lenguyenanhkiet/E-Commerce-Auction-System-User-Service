namespace ECommerceAuction.UserService.Domain.Repositories;

/// <summary>
/// Bulk maintenance operations run by background jobs.
/// </summary>
public interface IAccountMaintenanceRepository
{
    /// <summary>
    /// Reactivates locked accounts whose lockout window has expired and clears their failed-login counter.
    /// Returns the number of accounts unlocked.
    /// </summary>
    Task<int> UnlockExpiredLockoutsAsync(DateTimeOffset nowUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flags local-password accounts whose password is older than the threshold to change it on next use.
    /// Returns the number of accounts flagged.
    /// </summary>
    Task<int> FlagExpiredPasswordsAsync(DateTimeOffset passwordChangedBeforeUtc, CancellationToken cancellationToken = default);
}
