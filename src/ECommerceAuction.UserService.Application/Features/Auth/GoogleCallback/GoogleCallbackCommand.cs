using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.GoogleCallback;

/// <summary>
/// Represents the query values returned by Google after the user completes or cancels OAuth login.
/// </summary>
public sealed record GoogleCallbackCommand(
    string? Code,
    string? State,
    string? Error) : ICommand<GoogleCallbackResponse>;

/// <summary>
/// Contains the frontend URL that the backend should redirect the browser to after processing Google callback.
/// </summary>
public sealed record GoogleCallbackResponse(string FrontendRedirectUrl);

/// <summary>
/// Represents Google token endpoint response values used by the backend.
/// </summary>
public sealed record GoogleTokenResult(
    string AccessToken,
    string IdToken,
    int ExpiresIn,
    string TokenType);

/// <summary>
/// Represents verified identity information extracted from the Google ID token.
/// </summary>
public sealed record GoogleUserInfo(
    string ProviderUserId,
    string Email,
    bool EmailVerified,
    string? FullName,
    string? PictureUrl);
