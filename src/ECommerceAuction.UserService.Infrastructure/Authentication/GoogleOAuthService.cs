using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Auth.GoogleCallback;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Integrates with Google OAuth2/OpenID Connect endpoints.
/// </summary>
public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private static readonly string[] ValidIssuers =
    [
        "https://accounts.google.com",
        "accounts.google.com"
    ];

    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthOptions _options;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    public GoogleOAuthService(HttpClient httpClient, IOptions<GoogleOAuthOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            "https://accounts.google.com/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
    }

    /// <summary>
    /// Builds the Google authorization URL used to start the login redirect flow.
    /// </summary>
    public string BuildAuthorizationUrl(string state)
    {
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["prompt"] = "select_account"
        };

        return QueryHelpers.AddQueryString(_options.AuthorizationEndpoint, query);
    }

    /// <summary>
    /// Builds the frontend callback URL that receives the short-lived backend login code.
    /// </summary>
    public string BuildFrontendSuccessUrl(string code)
    {
        return QueryHelpers.AddQueryString(_options.FrontendSuccessUrl, "code", code);
    }

    /// <summary>
    /// Builds the frontend callback URL used when Google login fails.
    /// </summary>
    public string BuildFrontendFailureUrl(string errorCode)
    {
        return QueryHelpers.AddQueryString(_options.FrontendFailureUrl, "error", errorCode);
    }

    /// <summary>
    /// Exchanges Google's authorization code for Google tokens.
    /// </summary>
    public async Task<GoogleTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        });

        using var response = await _httpClient.PostAsync(
            _options.TokenEndpoint,
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google token response is empty.");

        if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
        {
            throw new InvalidOperationException("Google id_token is missing.");
        }

        return new GoogleTokenResult(
            tokenResponse.AccessToken,
            tokenResponse.IdToken,
            tokenResponse.ExpiresIn,
            tokenResponse.TokenType);
    }

    /// <summary>
    /// Validates Google's id_token and extracts trusted Google profile claims.
    /// </summary>
    public async Task<GoogleUserInfo> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = ValidIssuers,
            ValidateAudience = true,
            ValidAudience = _options.ClientId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(idToken, validationParameters, out _);

        var providerUserId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new SecurityTokenException("Google subject claim is missing.");

        var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? throw new SecurityTokenException("Google email claim is missing.");

        var emailVerifiedValue = principal.FindFirstValue("email_verified");
        var emailVerified = string.Equals(emailVerifiedValue, "true", StringComparison.OrdinalIgnoreCase);
        var fullName = principal.FindFirstValue("name");
        var pictureUrl = principal.FindFirstValue("picture");

        return new GoogleUserInfo(
            providerUserId,
            email.Trim().ToLowerInvariant(),
            emailVerified,
            fullName,
            pictureUrl);
    }

    /// <summary>
    /// Represents the JSON payload returned by Google's token endpoint.
    /// </summary>
    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string IdToken { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = string.Empty;
    }
}
