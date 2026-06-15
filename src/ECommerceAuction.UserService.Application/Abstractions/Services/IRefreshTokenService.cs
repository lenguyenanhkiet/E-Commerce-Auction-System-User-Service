namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Creates, hashes, and calculates expiration for refresh tokens stored in user sessions.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a cryptographically strong refresh token for a user session.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Hashes a refresh token before it is stored or used for session lookup.
    /// </summary>
    string HashToken(string refreshToken);

    /// <summary>
    /// Gets the UTC expiration time for a newly issued refresh token.
    /// </summary>
    DateTime GetRefreshTokenExpiresAt();
}
