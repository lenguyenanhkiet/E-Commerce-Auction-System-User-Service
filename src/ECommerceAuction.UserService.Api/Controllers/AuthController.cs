//Enter the necessary namespaces
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

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
/// Exposes authentication endpoints for local login/register, Google OAuth2, refresh token, and logout.
///Provides authentication endpoints for local login/registration, Google OAuth2, token refresh, and logout
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/v1/auth")] //Defines a root route for all authenticated endpoints
public class AuthController : ControllerBase
{
    //ISender object from MediatR, used to send Queries and Commands
    private readonly ISender _sender;

    //Constructor - initialization function, receives ISender via dependency injection
    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    ///POST /api/v1/auth/login - Log in the user using a local account
    ///Check if the user has a local account and validate the password
    /// </summary>
    [HttpPost("login")] //Identify this as the POST endpoint at route /api/v1/auth/login
    public async Task<IActionResult> CheckLocalAccount(
        [FromBody] CheckLocalAccountCommand command, //Get command from request body containing email and password
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send command to check local account through MediatR
        var result = await _sender.Send(command, cancellationToken);

        //Returns HTTP 200 OK with login results
        return Ok(new
        {
            //Notice of successful login
            message = "A user has successfully logged into the system",
            //The data contains JWT token and user information
            data = result
        });
    }

    /// <summary>
    ///POST /api/v1/auth/register - Register a new account
    ///Create a temporary account, send OTP via email for verification
    /// </summary>
    [HttpPost("register")] //Identify this as the POST endpoint at route /api/v1/auth/register
    public async Task<IActionResult> Register(
        [FromBody] RegisterAccountCommand command, //Get command from request body containing email, password, etc.
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send account registration command through MediatR
        var result = await _sender.Send(command, cancellationToken);

        //Returns HTTP 200 OK with registration results
        return Ok(new
        {
            //Notice of successful registration
            message = "A new user account has been successfully created",
            //The data contains temporary user ID and other information
            data = result
        });
    }

    /// <summary>
    ///POST /api/v1/auth/verify-email - Verify email using OTP
    ///Pass - VerifyEmail: verifies registration OTP and creates the real SQL user account.
    ///Verify registration OTP and create actual user account in database
    /// </summary>
    [HttpPost("verify-email")] //Identify this as the POST endpoint at route /api/v1/auth/verify-email
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command, //Get command from request body containing OTP
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send email verification command through MediatR
        var userId = await _sender.Send(command, cancellationToken);

        //Returns HTTP 200 OK with verification results
        return Ok(new
        {
            //Verified email notification
            message = "The email has been verified and the user account has been created.",
            //The data contains the created user ID
            data = new
            {
                userId
            }
        });
    }

    /// <summary>
    ///GET /api/v1/auth/google/login - Starts the Google login process
    /// ECA-6 OAuth2 Google: starts backend-owned Google login by redirecting the browser to Google.
    ///Redirect the browser to Google for the user to sign in
    /// </summary>
    [HttpGet("google/login")] //Identify this as the GET endpoint at route /api/v1/auth/google/login
    public async Task<IActionResult> GoogleLogin(CancellationToken cancellationToken)
    {
        //Sending a query starts the Google sign-in process through MediatR
        var result = await _sender.Send(new StartGoogleLoginQuery(), cancellationToken);

        //Redirect browser to Google OAuth2 URL
        return Redirect(result.RedirectUrl);
    }

    /// <summary>
    ///GET /api/v1/auth/google/callback - Receive callback from Google after user logs in
    /// ECA-6 OAuth2 Google: receives Google's callback, links or creates the local account, then redirects to FE.
    ///Receive a callback from Google, link or create a local account, then redirect to the frontend
    /// </summary>
    [HttpGet("google/callback")] //Identify this as the GET endpoint at route /api/v1/auth/google/callback
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? code, //Authorization code from Google
        [FromQuery] string? state, //Status to verify the integrity of the request
        [FromQuery] string? error, //Error message if any
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send commands to handle Google callbacks through MediatR
        var result = await _sender.Send(
            new GoogleCallbackCommand(code, state, error),
            cancellationToken);

        //Redirect to frontend with login information
        return Redirect(result.FrontendRedirectUrl);
    }

    /// <summary>
    ///POST /api/v1/auth/exchange-code - Exchange login code for JWT token
    /// ECA-6 OAuth2 Google: exchanges the short-lived login code returned to FE for application JWT tokens.
    ///Exchange the short-lived login code returned to the frontend for the application's JWT token
    /// </summary>
    [HttpPost("exchange-code")] //Identify this as the POST endpoint at route /api/v1/auth/exchange-code
    public async Task<IActionResult> ExchangeCode(
        [FromBody] ExchangeLoginCodeRequest request, //Get request from body containing login code
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send command to exchange code for JWT token via MediatR
        var result = await _sender.Send(
            new ExchangeLoginCodeCommand(request.Code),
            cancellationToken);

        //Returns HTTP 200 OK with JWT token
        return Ok(result);
    }

    /// <summary>
    ///POST /api/v1/auth/refresh-token - Refresh access token with refresh token
    /// ECA-17 Refresh token: rotates a refresh token and returns a new token pair.
    ///Rotate the refresh token and return a new token pair (access token and new refresh token)
    /// </summary>
    [HttpPost("refresh-token")] //Identify this as the POST endpoint at route /api/v1/auth/refresh-token
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request, //Get the request from the body containing the old refresh token
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send token refresh command via MediatR
        var result = await _sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        //Returns HTTP 200 OK with the new token pair
        return Ok(result);
    }

    /// <summary>
    ///POST /api/v1/auth/logout - Logs the user out
    /// ECA-10 Logout: revokes the current access token and optionally revokes the refresh session.
    ///Revoke the current access token and optionally revoke the session refresh token
    /// </summary>
    [Authorize] //Require users to authenticate (have a valid JWT token)
    [HttpPost("logout")] //Identify this as the POST endpoint at route /api/v1/auth/logout
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? request, //Get request from body containing refresh token (optional)
        CancellationToken cancellationToken) //Token to cancel the task if needed
    {
        //Send logout command via MediatR
        var result = await _sender.Send(
            new LogoutCommand(request?.RefreshToken),
            cancellationToken);

        //Returns HTTP 200 OK with logout result
        return Ok(result);
    }
}

/// <summary>
/// Request body used by FE to exchange a backend login code for JWT tokens.
///The request body is used by the frontend to exchange the backend login code for a JWT token
/// </summary>
public sealed record ExchangeLoginCodeRequest(
    string Code //Login code from backend in exchange for JWT token
);

/// <summary>
/// Request body used by FE to rotate a refresh token.
///The request body is used by the frontend to rotate the refresh token
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken //Refresh the old token to get a new pair of tokens
);

/// <summary>
/// Request body used by FE to revoke refresh-token session during logout.
///The request body is used by the frontend to retrieve the session refresh token upon logout
/// </summary>
public sealed record LogoutRequest(
    string? RefreshToken //Refresh token to revoke session (optional)
);
