using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;

namespace ECommerceAuction.UserService.Application.Features.Auth.GoogleLogin;

/// <summary>
/// Creates a protected OAuth state value and builds the Google authorization URL for browser redirection.
/// </summary>
public sealed class StartGoogleLoginQueryHandler
    : IQueryHandler<StartGoogleLoginQuery, StartGoogleLoginResponse>
{
    private readonly IOAuthStateService _oauthStateService;
    private readonly IGoogleOAuthService _googleOAuthService;

    public StartGoogleLoginQueryHandler(
        IOAuthStateService oauthStateService,
        IGoogleOAuthService googleOAuthService)
    {
        _oauthStateService = oauthStateService;
        _googleOAuthService = googleOAuthService;
    }

    /// <summary>
    /// Generates a one-time OAuth state and returns the Google login redirect URL.
    /// </summary>
    public async Task<StartGoogleLoginResponse> Handle(
        StartGoogleLoginQuery request,
        CancellationToken cancellationToken)
    {
        var state = await _oauthStateService.CreateAsync(cancellationToken);
        var redirectUrl = _googleOAuthService.BuildAuthorizationUrl(state);

        return new StartGoogleLoginResponse(redirectUrl);
    }
}
