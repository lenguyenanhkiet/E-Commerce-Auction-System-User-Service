namespace ECommerceAuction.UserService.Domain.Users;

public interface IAccountMaintenanceRepository
{
    Task<int> UnlockExpiredLockoutsAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<int> FlagExpiredPasswordsAsync(
        DateTimeOffset passwordChangedBeforeUtc,
        CancellationToken cancellationToken = default);
}
