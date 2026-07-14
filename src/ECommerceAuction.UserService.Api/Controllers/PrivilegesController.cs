using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Application.Features.Roles.GetPrivileges;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/privileges")]
public sealed class PrivilegesController : ControllerBase
{
    private readonly ISender _sender;

    public PrivilegesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Roles.View)]
    public async Task<IActionResult> GetPrivileges(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetPrivilegesQuery(), cancellationToken));
    }
}
