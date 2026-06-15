using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Tracks revoked access tokens after logout.
/// </summary>
public sealed class TokenRevocationService : ITokenRevocationService
{
    private const string BearerPrefix = "Bearer ";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _cache;

    public TokenRevocationService(
        IHttpContextAccessor httpContextAccessor,
        IDistributedCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    /// <summary>
    /// Revokes the current request access token until its original JWT expiration time.
    /// </summary>
    public async Task RevokeCurrentAccessTokenAsync(CancellationToken cancellationToken)
    {
        // ECA-10 Logout: read the access token from the current Authorization header.
        var accessToken = GetCurrentBearerToken()
            ?? throw new UnauthorizedAccessException("Access token is missing.");

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var timeToLive = jwtToken.ValidTo - DateTime.UtcNow;

        if (timeToLive <= TimeSpan.Zero)
        {
            return;
        }

        await _cache.SetStringAsync(
            BuildRevokedTokenKey(accessToken),
            "revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            },
            cancellationToken);
    }

    /// <summary>
    /// Checks whether an access token has been revoked by a previous logout operation.
    /// </summary>
    public async Task<bool> IsAccessTokenRevokedAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        var value = await _cache.GetStringAsync(
            BuildRevokedTokenKey(accessToken),
            cancellationToken);

        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Reads the bearer token from the current HTTP Authorization header.
    /// </summary>
    private string? GetCurrentBearerToken()
    {
        var authorization = _httpContextAccessor.HttpContext?
            .Request
            .Headers
            .Authorization
            .ToString();

        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorization[BearerPrefix.Length..].Trim();
    }

    /// <summary>
    /// Builds the cache key used to store a revoked access token hash.
    /// </summary>
    private static string BuildRevokedTokenKey(string accessToken)
    {
        return $"auth:revoked-access-token:{HashToken(accessToken)}";
    }

    /// <summary>
    /// Hashes an access token so the raw JWT is not stored in cache.
    /// </summary>
    private static string HashToken(string accessToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
