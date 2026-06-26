using ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;
using ECommerceAuction.UserService.Application.Features.Auth.ExchangeLoginCode;
using ECommerceAuction.UserService.Application.Features.Auth.GoogleCallback;
using ECommerceAuction.UserService.Application.Features.Auth.GoogleLogin;
using ECommerceAuction.UserService.Application.Features.Auth.Logout;
using ECommerceAuction.UserService.Application.Features.Auth.RefreshToken;
using ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;
using ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Exposes authentication endpoints for local login/register, Google OAuth2, refresh token, and logout.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    public async Task<IActionResult> CheckLocalAccount(
        [FromBody] CheckLocalAccountCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "A user has successfully logged into the system",
            data = result
        });
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

    /// <summary>
    /// Verifies the registration OTP and creates the real SQL user account.
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var userId = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "The email has been verified and the user account has been created.",
            data = new
            {
                userId
            }
        });
    }

    /// <summary>
    /// ECA-6 OAuth2 Google: starts backend-owned Google login by redirecting the browser to Google.
    /// </summary>
    [HttpGet("google/login")]
    public async Task<IActionResult> GoogleLogin(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartGoogleLoginQuery(), cancellationToken);

        return Redirect(result.RedirectUrl);
    }

    /// <summary>
    /// ECA-6 OAuth2 Google: receives Google's callback, links or creates the local account, then redirects to FE.
    /// </summary>
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GoogleCallbackCommand(code, state, error),
            cancellationToken);

        return Redirect(result.FrontendRedirectUrl);
    }

    /// <summary>
    /// ECA-6 OAuth2 Google: exchanges the short-lived login code returned to FE for application JWT tokens.
    /// </summary>
    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode(
        [FromBody] ExchangeLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ExchangeLoginCodeCommand(request.Code),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// ECA-17 Refresh token: rotates a refresh token and returns a new token pair.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// ECA-10 Logout: revokes the current access token and optionally revokes the refresh session.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LogoutCommand(request?.RefreshToken),
            cancellationToken);

        return Ok(result);
    }
}

/// <summary>
/// Request body used by FE to exchange a backend login code for JWT tokens.
/// </summary>
public sealed record ExchangeLoginCodeRequest(string Code);

/// <summary>
/// Request body used by FE to rotate a refresh token.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Request body used by FE to revoke refresh-token session during logout.
/// </summary>
public sealed record LogoutRequest(string? RefreshToken);
