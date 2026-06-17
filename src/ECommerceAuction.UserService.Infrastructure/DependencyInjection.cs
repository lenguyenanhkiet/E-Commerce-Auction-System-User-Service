using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using ECommerceAuction.UserService.Infrastructure.Caching;
using ECommerceAuction.UserService.Infrastructure.CurrentUser;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();
        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();
        services.AddScoped<IOAuthStateService, OAuthStateService>();
        services.AddScoped<ILoginCodeService, LoginCodeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
