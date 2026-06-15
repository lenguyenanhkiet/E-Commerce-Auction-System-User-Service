using ECommerceAuction.UserService.Application.Features.Auth.GoogleCallback;

namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Defines Google OAuth2 operations used by the application layer without depending on Google SDK or HTTP details.
/// </summary>
public interface IGoogleOAuthService
{
    /// <summary>
    /// Builds the Google authorization URL that the browser must be redirected to.
    /// </summary>
    string BuildAuthorizationUrl(string state);

    /// <summary>
    /// Builds the frontend callback URL used when Google login succeeds.
    /// </summary>
    string BuildFrontendSuccessUrl(string code);

    /// <summary>
    /// Builds the frontend login URL used when Google login fails.
    /// </summary>
    string BuildFrontendFailureUrl(string errorCode);

    /// <summary>
    /// Exchanges the Google authorization code for Google tokens.
    /// </summary>
    Task<GoogleTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the Google ID token and extracts verified Google user information.
    /// </summary>
    Task<GoogleUserInfo> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken);
}
