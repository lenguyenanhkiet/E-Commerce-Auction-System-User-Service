namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Issues short-lived service-to-service JWTs (token_use=service) for internal
/// clients such as the Catalog Service.
/// </summary>
public interface IServiceTokenIssuer
{
    /// <summary>
    /// Validates the client credentials, requested audience and scope, and issues a
    /// signed service token. Returns <c>null</c> when the credentials, audience or
    /// scope are not permitted.
    /// </summary>
    ServiceTokenResult? Issue(string clientId, string clientSecret, string audience, string scope);
}

public sealed record ServiceTokenResult(string AccessToken, int ExpiresInSeconds);
