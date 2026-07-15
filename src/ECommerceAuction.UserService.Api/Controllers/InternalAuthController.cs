using ECommerceAuction.UserService.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Issues service-to-service access tokens for internal clients (e.g. Catalog Service).
/// </summary>
[ApiController]
[Route("api/v1/internal/auth")]
public sealed class InternalAuthController : ControllerBase
{
    private readonly IServiceTokenIssuer _tokenIssuer;

    public InternalAuthController(IServiceTokenIssuer tokenIssuer)
    {
        _tokenIssuer = tokenIssuer;
    }

    /// <summary>
    /// POST /api/v1/internal/auth/token — exchange client credentials for a service token.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult IssueToken([FromBody] ServiceTokenRequest request)
    {
        var result = _tokenIssuer.Issue(
            request.ClientId,
            request.ClientSecret,
            request.Audience,
            request.Scope);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid client credentials, audience or scope.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            message = "Service token issued.",
            data = new
            {
                accessToken = result.AccessToken,
                expiresIn = result.ExpiresInSeconds
            }
        });
    }
}

public sealed record ServiceTokenRequest(
    string ClientId,
    string ClientSecret,
    string Audience,
    string Scope);
