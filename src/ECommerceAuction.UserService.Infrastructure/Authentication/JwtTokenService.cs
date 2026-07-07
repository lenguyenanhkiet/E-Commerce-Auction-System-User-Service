using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceAuction.UserService.Infrastructure.Authentication;

/// <summary>
/// Generates application JWT access tokens for authenticated users.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Keeps compatibility with the existing normal login task by returning only the raw token value.
    /// </summary>
    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        return GenerateAccessToken(userId, email, roles, privileges: []).Value;
    }

    /// <summary>
    /// Creates a signed JWT access token with user, role, and privilege claims.
    /// </summary>
    public JwtAccessToken GenerateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string> privileges)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);

        // ECA-6 OAuth2 Google: keep JWT claim format aligned with the shared JWT service.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_use", "user"),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(privileges.Select(privilege => new Claim("privilege", privilege)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
