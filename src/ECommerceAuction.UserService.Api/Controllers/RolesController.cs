using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Application.Features.Roles.CreateRole;
using ECommerceAuction.UserService.Application.Features.Roles.DeleteRole;
using ECommerceAuction.UserService.Application.Features.Roles.GetRoleById;
using ECommerceAuction.UserService.Application.Features.Roles.GetRoles;
using ECommerceAuction.UserService.Application.Features.Roles.UpdateRole;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Exposes privilege-protected Role Management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Roles.List)]
    public async Task<IActionResult> GetRoles(
        [FromQuery] GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet("{roleId:guid}")]
    [Authorize(Policy = Permissions.Roles.View)]
    public async Task<IActionResult> GetRoleById(Guid roleId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetRoleByIdQuery(roleId), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Roles.Create)]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Id }, result);
    }

    [HttpPut("{roleId:guid}")]
    [Authorize(Policy = Permissions.Roles.Update)]
    public async Task<IActionResult> UpdateRole(
        Guid roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand(
            roleId,
            request.Name,
            request.Description,
            request.PrivilegeCodes);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{roleId:guid}")]
    [Authorize(Policy = Permissions.Roles.Delete)]
    public async Task<IActionResult> DeleteRole(Guid roleId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRoleCommand(roleId), cancellationToken);
        return NoContent();
    }
}
