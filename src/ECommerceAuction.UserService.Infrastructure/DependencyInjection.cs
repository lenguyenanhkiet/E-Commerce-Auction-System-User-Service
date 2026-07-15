using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using ECommerceAuction.UserService.Infrastructure.Caching;
using ECommerceAuction.UserService.Infrastructure.CurrentUser;
using ECommerceAuction.UserService.Infrastructure.HostedServices;
using ECommerceAuction.UserService.Infrastructure.IdentityVerification;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure;

/// <summary>
/// Registers infrastructure services such as JWT authentication, Google OAuth, cache, and event bus.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer dependencies to the application service container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleOAuthOptions>(configuration.GetSection(GoogleOAuthOptions.SectionName));

        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        // ECA-10 Logout: reject access tokens that were revoked by POST /api/v1/auth/logout.
                        if (context.SecurityToken is not JwtSecurityToken jwtToken ||
                            string.IsNullOrWhiteSpace(jwtToken.RawData))
                        {
                            return;
                        }

                        var tokenRevocationService = context.HttpContext
                            .RequestServices
                            .GetRequiredService<ITokenRevocationService>();

                        var isRevoked = await tokenRevocationService.IsAccessTokenRevokedAsync(
                            jwtToken.RawData,
                            context.HttpContext.RequestAborted);

                        if (isRevoked)
                        {
                            context.Fail("Access token was revoked.");
                        }
                    }
                };
            })
            .AddJwtBearer(ServiceAuthSchemes.ServiceJwt, options =>
            {
                // Validates internal service-to-service tokens (token_use=service). Same
                // signing key/issuer as user tokens, but a distinct internal audience.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.InternalAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        //Configure MassTransit to use RabbitMq as the message broker
        //MassTransit is a library that helps handle sending/receiving messages from queues
        services.AddMassTransit(x =>
        {
            //Configure to use RabbitMq as service transport
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "rabbitmq";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";
                var vhost = configuration["RabbitMQ:VHost"] ?? "/";
                var useSsl = !string.Equals(host, "rabbitmq", StringComparison.OrdinalIgnoreCase);

                var scheme = useSsl ? "amqps" : "amqp";
                var port = useSsl ? 5671 : 5672;
                var hostUri = new Uri($"{scheme}://{host}:{port}/{Uri.EscapeDataString(vhost)}");

                cfg.Host(hostUri, h =>
                {
                    h.Username(username);
                    h.Password(password);
                    if (useSsl)
                    {
                        h.UseSsl(ssl =>
                        {
                            ssl.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                            ssl.ServerName = host;
                        });
                    }
                });
                //Configure endpoints to automatically map messages
                cfg.ConfigureEndpoints(context);
            });
        });

        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UserService_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddHttpContextAccessor();

        services.Configure<InternalAuthOptions>(
            configuration.GetSection(InternalAuthOptions.SectionName));

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IServiceTokenIssuer, ServiceTokenIssuer>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();
        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();
        services.AddScoped<IOAuthStateService, OAuthStateService>();
        services.AddScoped<ILoginCodeService, LoginCodeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IIdentityVerificationProvider, MockIdentityVerificationProvider>();
        services.AddHostedService<DatabaseMigrationService>();

        return services;
    }
}