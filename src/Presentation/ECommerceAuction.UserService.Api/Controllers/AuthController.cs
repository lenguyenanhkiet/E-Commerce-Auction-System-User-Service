using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;


namespace ECommerceAuction.UserService.Api.Controllers;


[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

   

    [HttpPost("register")]
    public async Task<IActionResult> Register(
    [FromBody] RegisterAccountCommand command,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "A new user account has been successfully created",
            data = result
        });
    }

   
}