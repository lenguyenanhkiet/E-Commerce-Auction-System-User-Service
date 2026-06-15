namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Contains Google OAuth2/OpenID Connect configuration values.
/// </summary>
public sealed class GoogleOAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string AuthorizationEndpoint { get; init; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string TokenEndpoint { get; init; } = "https://oauth2.googleapis.com/token";
    public string RedirectUri { get; init; } = string.Empty;
    public string FrontendSuccessUrl { get; init; } = string.Empty;
    public string FrontendFailureUrl { get; init; } = string.Empty;
}
