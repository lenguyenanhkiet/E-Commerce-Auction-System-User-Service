namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Tracks revoked access tokens so logout can invalidate a JWT before its natural expiration.
/// </summary>
public interface ITokenRevocationService
{
    /// <summary>
    /// Revokes the current bearer access token from the active HTTP request.
    /// </summary>
    Task RevokeCurrentAccessTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether an access token has already been revoked.
    /// </summary>
    Task<bool> IsAccessTokenRevokedAsync(string accessToken, CancellationToken cancellationToken);
}
