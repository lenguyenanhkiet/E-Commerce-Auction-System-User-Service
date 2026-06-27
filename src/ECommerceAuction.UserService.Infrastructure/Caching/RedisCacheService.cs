using System.Text.Json;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceAuction.UserService.Infrastructure.Caching;

/// <summary>
/// Stores pending registration data in Redis until email OTP verification succeeds.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Serializes a value as JSON and stores it with the requested expiration.
    /// </summary>
    public async Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            },
            cancellationToken);
    }

    /// <summary>
    /// Reads a JSON value from cache and deserializes it to the requested type.
    /// </summary>
    public async Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TValue>(json, SerializerOptions);
    }

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(key, cancellationToken);
    }
}
