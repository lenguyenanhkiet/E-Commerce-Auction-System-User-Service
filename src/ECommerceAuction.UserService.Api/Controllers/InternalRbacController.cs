using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Internal endpoint other services call at startup to declare the privileges they own
/// and how those map onto roles. Service-to-service only — never exposed to end users.
/// </summary>
[ApiController]
[Route("api/v1/internal/rbac")]
[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class InternalRbacController : ControllerBase
{
    private const string WriteScope = "user.internal.rbac.write";

    private readonly IRbacRegistrar _registrar;
    private readonly ILogger<InternalRbacController> _logger;

    public InternalRbacController(IRbacRegistrar registrar, ILogger<InternalRbacController> logger)
    {
        _registrar = registrar;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/v1/internal/rbac/privileges — register privileges and role grants.
    /// Idempotent: services call this on every startup.
    /// </summary>

    [HttpPost("privileges")]
    public async Task<IActionResult> RegisterPrivileges([FromBody] RbacRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!HasWriteScope())
        {
            _logger.LogWarning(
                "RBAC registration refused for '{ServiceCode}': token lacks scope '{Scope}'.",
                request.ServiceCode, WriteScope);
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.ServiceCode))
        {
            return BadRequest(new { message = "A service code is required." });
        }

        await _registrar.RegisterAsync(request, cancellationToken);

        return Ok(new { message = $"RBAC registration accepted for '{request.ServiceCode}'." });
    }

    /// <summary>
    /// The scope claim holds space-delimited values, so a token may carry several scopes.
    /// </summary>
    private bool HasWriteScope()
    {
        // The service scheme sets MapInboundClaims = false, so the claim stays "scope".
        var scopes = User.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return scopes.Contains(WriteScope, StringComparer.Ordinal);
    }
}