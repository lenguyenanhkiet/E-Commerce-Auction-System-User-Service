using ECommerceAuction.UserService.Application.Common.Options;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Api.BackgroundJobs;

/// <summary>
/// Periodically reactivates accounts whose temporary lockout window has expired,
/// so the stored status reflects reality without waiting for the next login attempt.
/// </summary>
public sealed class AccountUnlockSweeperService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AccountPolicyOptions _options;
    private readonly ILogger<AccountUnlockSweeperService> _logger;

    public AccountUnlockSweeperService(
        IServiceScopeFactory scopeFactory,
        IOptions<AccountPolicyOptions> options,
        ILogger<AccountUnlockSweeperService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(30, _options.UnlockSweepIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAccountMaintenanceRepository>();

                var unlocked = await repository.UnlockExpiredLockoutsAsync(DateTimeOffset.UtcNow, stoppingToken);

                if (unlocked > 0)
                {
                    _logger.LogInformation("Account unlock sweep reactivated {Count} account(s).", unlocked);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // A failed sweep must never crash the host; the next tick retries.
                _logger.LogError(exception, "Account unlock sweep failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
