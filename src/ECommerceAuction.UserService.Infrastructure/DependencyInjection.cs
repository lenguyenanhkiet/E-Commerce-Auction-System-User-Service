using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Services.IdentityMatching;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using ECommerceAuction.UserService.Infrastructure.Caching;
using ECommerceAuction.UserService.Infrastructure.CurrentUser;
using ECommerceAuction.UserService.Infrastructure.HostedServices;
using ECommerceAuction.UserService.Infrastructure.IdentityVerification;
using ECommerceAuction.UserService.Infrastructure.Messaging;
using ECommerceAuction.UserService.Infrastructure.PaymentMethods;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.Ocr.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure;

/// <summary>
/// Registers infrastructure services such as authentication,
/// cache, external providers, hosted services, and messaging.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureAuthentication(
            services,
            configuration);

        services.AddMessaging(configuration);

        ConfigureCaching(
            services,
            configuration);

        services.AddHttpContextAccessor();

        services.Configure<InternalAuthOptions>(
            configuration.GetSection(
                InternalAuthOptions.SectionName));

        services.AddScoped<
            ICacheService,
            RedisCacheService>();

        services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        services.AddScoped<
            IServiceTokenIssuer,
            ServiceTokenIssuer>();

        services.AddScoped<
            IRefreshTokenService,
            RefreshTokenService>();

        services.AddScoped<
            ITokenRevocationService,
            TokenRevocationService>();

        services.AddHttpClient<
            IGoogleOAuthService,
            GoogleOAuthService>();

        services.AddScoped<
            IOAuthStateService,
            OAuthStateService>();

        services.AddScoped<
            ILoginCodeService,
            LoginCodeService>();

        services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();

        services.AddScoped<
            IPasswordHasher,
            PasswordHasher>();

        services.AddHostedService<
            DatabaseMigrationService>();

        services.Configure<BankVerificationOptions>(
            configuration.GetSection(
                BankVerificationOptions.SectionName));

        services.AddSingleton<
            BankVerificationSignatureValidator>();

        services.AddScoped<
            IBankVerificationProvider,
            BankVerificationProvider>();

        AddIdentityVerification(
            services,
            configuration);

        return services;
    }

    private static void ConfigureAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.Configure<GoogleOAuthOptions>(
            configuration.GetSection(
                GoogleOAuthOptions.SectionName));

        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration was not found.");

        if (string.IsNullOrWhiteSpace(
                jwtOptions.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT secret key is required.");
        }

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtOptions.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.SecurityToken
                                is not JwtSecurityToken jwtToken ||
                            string.IsNullOrWhiteSpace(
                                jwtToken.RawData))
                        {
                            return;
                        }

                        var tokenRevocationService =
                            context.HttpContext
                                .RequestServices
                                .GetRequiredService<
                                    ITokenRevocationService>();

                        var isRevoked =
                            await tokenRevocationService
                                .IsAccessTokenRevokedAsync(
                                    jwtToken.RawData,
                                    context.HttpContext
                                        .RequestAborted);

                        if (isRevoked)
                        {
                            context.Fail(
                                "Access token was revoked.");
                        }
                    }
                };
            })
            .AddJwtBearer(
                ServiceAuthSchemes.ServiceJwt,
                options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtOptions.Issuer,
                            ValidAudience =
                                jwtOptions.InternalAudience,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SecretKey)),
                            ClockSkew = TimeSpan.Zero
                        };
                });
    }

    private static void ConfigureCaching(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString =
            configuration.GetConnectionString(
                "RedisConnection");

        if (!string.IsNullOrWhiteSpace(
                redisConnectionString))
        {
            services.AddStackExchangeRedisCache(
                options =>
                {
                    options.Configuration =
                        redisConnectionString;

                    options.InstanceName =
                        "UserService_";
                });

            return;
        }

        services.AddDistributedMemoryCache();
    }

    /// <summary>
    /// Wires identity verification. The mock reads no image
    /// and should only be used in development.
    /// </summary>
    private static void AddIdentityVerification(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdentityVerificationOptions>(
            configuration.GetSection(
                IdentityVerificationOptions.SectionName));

        services.Configure<IdentityMatchingOptions>(
            configuration.GetSection(
                IdentityMatchingOptions.SectionName));

        var options = configuration
            .GetSection(
                IdentityVerificationOptions.SectionName)
            .Get<IdentityVerificationOptions>()
            ?? new IdentityVerificationOptions();

        if (options.UseMockProvider)
        {
            services.AddScoped<
                IIdentityVerificationProvider,
                MockIdentityVerificationProvider>();

            return;
        }

        var tessDataPath =
            string.IsNullOrWhiteSpace(
                options.TessDataPath)
                ? Path.Combine(
                    AppContext.BaseDirectory,
                    "tessdata")
                : options.TessDataPath;

        services.AddNexusOcr(tessDataPath);

        services.AddScoped<
            IIdentityVerificationProvider,
            NexusOcrIdentityVerificationProvider>();
    }
}