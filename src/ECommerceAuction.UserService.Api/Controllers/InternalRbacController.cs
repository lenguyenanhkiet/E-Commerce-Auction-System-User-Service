using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Internal endpoint other services call at startup to declare the privileges they own
/// and how those privileges map onto system roles. This endpoint is service-to-service only.
/// </summary>
[ApiController]
[Route("api/v1/internal/rbac")]
[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class InternalRbacController : ControllerBase
{
    private const string WriteScope = "user.internal.rbac.write";

    private readonly IRbacRegistrar _registrar;
    private readonly ILogger<InternalRbacController> _logger;

    public InternalRbacController(
        IRbacRegistrar registrar,
        ILogger<InternalRbacController> logger)
    {
        _registrar = registrar;
        _logger = logger;
    }

    /// <summary>
    /// Registers privileges and default system-role grants declared by one service.
    /// The operation is idempotent and may safely be called on every service startup.
    /// </summary>
    [HttpPost("privileges")]
    public async Task<IActionResult> RegisterPrivileges(
        [FromBody] RbacRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasWriteScope())
        {
            _logger.LogWarning(
                "RBAC registration refused for '{ServiceCode}': token lacks scope '{Scope}'.",
                request.ServiceCode,
                WriteScope);

            return Forbid();
        }

        if (request.Privileges is null || request.RoleGrants is null)
        {
            return BadRequest(new
            {
                message = "Privileges and RoleGrants are required."
            });
        }

        var serviceCode = NormalizeServiceCode(request.ServiceCode);
        if (serviceCode is null)
        {
            return BadRequest(new
            {
                message = "ServiceCode is required and may contain only letters, digits, or hyphens."
            });
        }

        var callerClientId = User.FindFirst("client_id")?.Value;
        var expectedClientId = $"{serviceCode}-service";

        if (!string.Equals(
                callerClientId,
                expectedClientId,
                StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "RBAC registration service mismatch. Client '{ClientId}' attempted to register service '{ServiceCode}'.",
                callerClientId,
                serviceCode);

            return Forbid();
        }

        var normalizedRequest = request with
        {
            ServiceCode = serviceCode
        };

        await _registrar.RegisterAsync(normalizedRequest, cancellationToken);

        return Ok(new
        {
            message = $"RBAC registration accepted for '{serviceCode}'."
        });
    }

    /// <summary>
    /// The scope claim holds space-delimited values, so a token may carry several scopes.
    /// </summary>
    private bool HasWriteScope()
    {
        var scopes = User.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries));

        return scopes.Contains(WriteScope, StringComparer.Ordinal);
    }

    /// <summary>
    /// Converts a service code into the canonical lowercase form used to bind
    /// serviceCode=commerce to client_id=commerce-service.
    /// </summary>
    private static string? NormalizeServiceCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized.All(character =>
                char.IsLetterOrDigit(character) || character == '-')
            ? normalized
            : null;
    }
}
