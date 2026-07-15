using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Lets internal services register the privilege codes they own (and role grants for them)
/// into the shared RBAC store. Requires a service token carrying the rbac.write scope.
/// </summary>
[ApiController]
[Route("api/v1/internal/rbac")]
[Authorize(AuthenticationSchemes = ServiceAuthSchemes.ServiceJwt)]
public sealed class InternalRbacController : ControllerBase
{
    public const string WriteScope = "user.internal.rbac.write";

    private readonly IRbacRegistrar _registrar;

    public InternalRbacController(IRbacRegistrar registrar)
    {
        _registrar = registrar;
    }

    /// <summary>
    /// POST /api/v1/internal/rbac/privileges — upsert privileges + role grants (idempotent).
    /// </summary>
    [HttpPost("privileges")]
    public async Task<IActionResult> RegisterPrivileges(
        [FromBody] RegisterPrivilegesRequest request,
        CancellationToken cancellationToken)
    {
        var hasScope = User.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Any(scope => string.Equals(scope, WriteScope, StringComparison.Ordinal));

        if (!hasScope)
        {
            return Forbid();
        }

        await _registrar.RegisterAsync(
            new RbacRegistrationRequest(
                request.ServiceCode ?? string.Empty,
                (request.Privileges ?? []).Select(p => new PrivilegeDefinition(p.Code, p.Description)).ToList(),
                (request.RoleGrants ?? []).Select(g => new RoleGrant(g.RoleCode, g.PrivilegeCode)).ToList()),
            cancellationToken);

        return Ok(new { message = "RBAC registration applied." });
    }
}

public sealed record RegisterPrivilegesRequest(
    string? ServiceCode,
    List<PrivilegeItem>? Privileges,
    List<RoleGrantItem>? RoleGrants);

public sealed record PrivilegeItem(string Code, string? Description);

public sealed record RoleGrantItem(string RoleCode, string PrivilegeCode);
