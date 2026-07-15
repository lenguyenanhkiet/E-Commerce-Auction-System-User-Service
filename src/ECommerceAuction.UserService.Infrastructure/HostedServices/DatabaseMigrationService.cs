using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceAuction.UserService.Infrastructure.HostedServices;

/// <summary>
/// On startup in deployed environments (Staging / Production), applies any pending EF Core
/// migrations and THEN seeds the permission catalog — in that order, before the app serves
/// traffic. Running from inside the container reaches the database over the app's own network,
/// so there is no external CI runner / firewall dependency.
/// </summary>
public sealed class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        ILogger<DatabaseMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Auto-migrate on every startup, in every environment. The docker-compose containers and
        // the deployed servers all rely on this, and the environment label is not a reliable gate
        // (local docker may run as Development). `dotnet ef` design-time commands do not start
        // hosted services, so they are unaffected.
        _logger.LogInformation(
            "Running startup database migration + permission seeding ({Environment}).",
            _environment.EnvironmentName);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Apply migrations. No artificial timeout: a cold serverless database can take a while
        //    to resume, and the DbContext's EnableRetryOnFailure policy handles transient errors.
        //    A genuine migration failure is intentionally allowed to propagate — the app must not
        //    start serving traffic against a schema that is not up to date.
        _logger.LogInformation(
            "Applying database migrations ({Environment})...",
            _environment.EnvironmentName);
        await dbContext.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migrations applied successfully.");

        // 2. Seed the permission catalog AFTER migrations, so the Privileges/Roles tables exist.
        //    Seeding is idempotent; a seeding hiccup should not take the whole service down.
        try
        {
            _logger.LogInformation("Seeding the permission catalog...");
            await PermissionSeeder.SeedAsync(dbContext, cancellationToken);
            _logger.LogInformation("Permission catalog seeded successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Permission catalog seeding failed. Permission-protected endpoints may reject valid requests until this is resolved.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}