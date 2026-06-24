using ECommerceAuction.UserService.Application.Features.Users.ChangePassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class ProfileController : ControllerBase
{
    private readonly ISender _sender;

    public ProfileController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "Password changed successfully.",
            data = result
        });
    }
}