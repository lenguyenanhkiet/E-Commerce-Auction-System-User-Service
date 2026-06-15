using System.Security.Cryptography;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Stores one-time login codes used to hand off Google login from backend to frontend.
/// </summary>
public sealed class LoginCodeService : ILoginCodeService
{
    private static readonly TimeSpan LoginCodeTtl = TimeSpan.FromMinutes(3);

    private readonly IDistributedCache _cache;

    public LoginCodeService(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Creates a short-lived one-time code for the authenticated user.
    /// </summary>
    public async Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var code = CreateSecureCode();
        var key = BuildKey(code);

        await _cache.SetStringAsync(
            key,
            userId.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = LoginCodeTtl
            },
            cancellationToken);

        return code;
    }

    /// <summary>
    /// Validates and removes a one-time login code so it cannot be reused.
    /// </summary>
    public async Task<Guid> ValidateAndConsumeAsync(string code, CancellationToken cancellationToken)
    {
        var key = BuildKey(code);
        var value = await _cache.GetStringAsync(key, cancellationToken);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Login code is invalid or expired.");
        }

        await _cache.RemoveAsync(key, cancellationToken);
        return userId;
    }

    /// <summary>
    /// Builds the cache key for a one-time login code.
    /// </summary>
    private static string BuildKey(string code)
    {
        return $"auth:login-code:{code}";
    }

    /// <summary>
    /// Creates a cryptographically secure URL-safe login code.
    /// </summary>
    private static string CreateSecureCode()
    {
        return Base64UrlTextEncoder.Encode(RandomNumberGenerator.GetBytes(32));
    }
}
