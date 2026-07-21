namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Registry of internal service clients allowed to obtain service-to-service tokens
/// from <c>POST /api/v1/internal/auth/token</c>. Bound from the "InternalAuth" config
/// section; secrets are supplied via environment variables.
/// </summary>
public sealed class InternalAuthOptions
{
    public const string SectionName = "InternalAuth";

    public List<InternalClient> Clients { get; init; } = new();
}

public sealed class InternalClient
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public List<string> AllowedAudiences { get; init; } = new();
    public List<string> AllowedScopes { get; init; } = new();
}
