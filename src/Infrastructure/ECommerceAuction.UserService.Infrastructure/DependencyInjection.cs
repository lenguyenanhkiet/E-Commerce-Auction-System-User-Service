using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using ECommerceAuction.UserService.Infrastructure.CurrentUser;
using ECommerceAuction.UserService.Infrastructure.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ECommerceAuction.UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      
       
        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UserService_";
            });
        }

        services.AddHttpContextAccessor();
       
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IEventBus, MockEventBus>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
}

