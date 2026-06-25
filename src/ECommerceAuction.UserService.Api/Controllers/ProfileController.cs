using ECommerceAuction.UserService.Application.Features.Users.ChangePassword;
using ECommerceAuction.UserService.Application.Features.Auth.ForgotPassword;
using ECommerceAuction.UserService.Application.Features.Auth.ResetPassword;
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

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
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

    /// <summary>
    /// Creates a password reset request for the specified email address.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordCommand command,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "Reset password request has been processed.",
            data = result
        });
    }

    /// <summary>
    /// Resets the user password using a valid password reset token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "Password has been reset successfully.",
            data = result
        });
    }
}