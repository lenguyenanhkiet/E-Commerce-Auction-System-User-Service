using System.Security.Cryptography;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Stores and validates OAuth state values to protect the Google login redirect flow.
/// </summary>
public sealed class OAuthStateService : IOAuthStateService
{
    private static readonly TimeSpan StateTtl = TimeSpan.FromMinutes(5);

    private readonly IDistributedCache _cache;

    public OAuthStateService(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Creates a short-lived state value before redirecting the user to Google.
    /// </summary>
    public async Task<string> CreateAsync(CancellationToken cancellationToken)
    {
        var state = CreateSecureCode();
        var key = BuildKey(state);

        await _cache.SetStringAsync(
            key,
            "valid",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = StateTtl
            },
            cancellationToken);

        return state;
    }

    /// <summary>
    /// Validates and removes a state value after Google redirects back to the backend.
    /// </summary>
    public async Task<bool> ValidateAndConsumeAsync(string state, CancellationToken cancellationToken)
    {
        var key = BuildKey(state);
        var value = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        await _cache.RemoveAsync(key, cancellationToken);
        return true;
    }

    /// <summary>
    /// Builds the cache key for a Google OAuth state value.
    /// </summary>
    private static string BuildKey(string state)
    {
        return $"oauth:google:state:{state}";
    }

    /// <summary>
    /// Creates a cryptographically secure URL-safe state value.
    /// </summary>
    private static string CreateSecureCode()
    {
        return Base64UrlTextEncoder.Encode(RandomNumberGenerator.GetBytes(32));
    }
}
