using ECommerceAuction.UserService.Application.Common.Options;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Api.BackgroundJobs;

/// <summary>
/// Periodically flags accounts whose local password is older than the configured expiry
/// so the user is required to change it (surfaced to the client on next login).
/// </summary>
public sealed class PasswordExpirySweeperService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AccountPolicyOptions _options;
    private readonly ILogger<PasswordExpirySweeperService> _logger;

    public PasswordExpirySweeperService(
        IServiceScopeFactory scopeFactory,
        IOptions<AccountPolicyOptions> options,
        ILogger<PasswordExpirySweeperService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(Math.Max(1, _options.PasswordExpirySweepIntervalHours));
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAccountMaintenanceRepository>();

                var threshold = DateTimeOffset.UtcNow.AddDays(-Math.Max(1, _options.PasswordExpiryDays));
                var flagged = await repository.FlagExpiredPasswordsAsync(threshold, stoppingToken);

                if (flagged > 0)
                {
                    _logger.LogInformation(
                        "Password expiry sweep flagged {Count} account(s) older than {Days} day(s).",
                        flagged, _options.PasswordExpiryDays);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Password expiry sweep failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
