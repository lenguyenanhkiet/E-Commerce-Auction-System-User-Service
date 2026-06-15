namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Provides JSON-based cache operations used by authentication flows.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Stores a value in cache with an absolute expiration time.
    /// </summary>
    Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads and deserializes a value from cache.
    /// </summary>
    Task<TValue?> GetAsync<TValue>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}
