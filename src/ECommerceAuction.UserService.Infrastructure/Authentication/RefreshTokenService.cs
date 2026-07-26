using System.Security.Cryptography;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Creates and hashes refresh tokens for persistent user sessions.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtOptions _options;

    public RefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    public string GenerateRefreshToken()
    {
        // ECA-17 Refresh token: use cryptographically strong random bytes for long-lived session tokens.
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Hashes a refresh token before it is stored or looked up in the database.
    /// </summary>
    public string HashToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Calculates the UTC expiration timestamp for a new refresh token.
    /// </summary>
    public DateTimeOffset GetRefreshTokenExpiresAt()
    {
        return DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpirationDays);
    }
}
