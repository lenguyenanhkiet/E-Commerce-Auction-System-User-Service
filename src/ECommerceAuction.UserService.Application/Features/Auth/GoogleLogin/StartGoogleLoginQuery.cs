using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.GoogleLogin;

/// <summary>
/// Starts the Google OAuth2 login flow and returns the Google authorization redirect URL.
/// </summary>
public sealed record StartGoogleLoginQuery : IQuery<StartGoogleLoginResponse>;

/// <summary>
/// Contains the URL that the browser should navigate to for Google login.
/// </summary>
public sealed record StartGoogleLoginResponse(string RedirectUrl);
