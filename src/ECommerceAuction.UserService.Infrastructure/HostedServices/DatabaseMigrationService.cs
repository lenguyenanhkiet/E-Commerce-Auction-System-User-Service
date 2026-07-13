using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure.HostedServices;

public class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IServiceProvider serviceProvider, IHostEnvironment environment, ILogger<DatabaseMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //if (!_environment.IsProduction())
        //{
        //    _logger.LogInformation("Skipping migration");
        //    return;
        //}

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            _logger.LogInformation("=== Running migrations ===");
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}