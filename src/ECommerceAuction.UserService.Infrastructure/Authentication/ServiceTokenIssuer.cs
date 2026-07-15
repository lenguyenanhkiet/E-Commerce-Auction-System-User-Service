using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Issues internal service-to-service tokens for clients configured under "InternalAuth".
/// </summary>
public sealed class ServiceTokenIssuer : IServiceTokenIssuer
{
    private readonly JwtOptions _jwt;
    private readonly InternalAuthOptions _internal;

    public ServiceTokenIssuer(
        IOptions<JwtOptions> jwtOptions,
        IOptions<InternalAuthOptions> internalOptions)
    {
        _jwt = jwtOptions.Value;
        _internal = internalOptions.Value;
    }

    public ServiceTokenResult? Issue(string clientId, string clientSecret, string audience, string scope)
    {
        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(scope))
        {
            return null;
        }

        // The requested audience must be this service's internal audience.
        if (!string.Equals(audience, _jwt.InternalAudience, StringComparison.Ordinal))
        {
            return null;
        }

        var client = _internal.Clients.FirstOrDefault(
            c => string.Equals(c.ClientId, clientId, StringComparison.Ordinal));

        // A request may ask for several space-delimited scopes; every one must be allowed.
        var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (client is null ||
            string.IsNullOrEmpty(client.ClientSecret) ||
            !FixedTimeEquals(client.ClientSecret, clientSecret) ||
            requestedScopes.Length == 0 ||
            requestedScopes.Any(s => !client.AllowedScopes.Contains(s, StringComparer.Ordinal)))
        {
            return null;
        }

        var expiresInSeconds = Math.Max(60, _jwt.ServiceTokenExpirationMinutes * 60);
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Resource services distinguish service tokens from user tokens via token_use.
            new("token_use", "service"),
            new("client_id", clientId),
            new("scope", scope)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.InternalAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new ServiceTokenResult(accessToken, expiresInSeconds);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b));
}
