using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceAuction.UserService.Persistence;

/// <summary>
/// Registers persistence services and repositories for User Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds EF Core SQL Server and repository dependencies.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserPasswordHistoryRepository, UserPasswordHistoryRepository>();
        services.AddScoped<IUserOAuthRepository, UserOAuthRepository>();
        services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();

        return services;
    }
}
