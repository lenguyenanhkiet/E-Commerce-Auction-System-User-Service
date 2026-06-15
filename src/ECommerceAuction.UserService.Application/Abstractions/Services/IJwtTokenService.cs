namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Generates system JWT access tokens after a user has been authenticated by any login flow.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Keeps compatibility with the existing normal login task that only needs the raw access token string.
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Creates a signed JWT access token using the shared claim format for roles and privileges.
    /// </summary>
    JwtAccessToken GenerateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> privileges);
}

/// <summary>
/// Represents a generated access token and its UTC expiration time.
/// </summary>
public sealed record JwtAccessToken(string Value, DateTime ExpiresAt);
