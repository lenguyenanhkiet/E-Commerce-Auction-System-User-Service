using ECommerceAuction.UserService.Api.Contracts.Addresses;
using ECommerceAuction.UserService.Api.Contracts.Users;
using ECommerceAuction.UserService.Application.Features.Users.DeleteAddress;
using ECommerceAuction.UserService.Application.Features.Users.GetProfile;
using ECommerceAuction.UserService.Application.Features.Users.RequestPhoneOtp;
using ECommerceAuction.UserService.Application.Features.Users.UpdateProfile;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.CreateAddress;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetAddressById;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.GetListAddresses;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.SetDefaultAddress;
using ECommerceAuction.UserService.Application.Features.Users.UserAddress.UpdateAddress;
using ECommerceAuction.UserService.Application.Features.Users.VerifyPhoneOtp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
///User profile endpoints - User profile management endpoints
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/v1/users")] //Defines a root route for all endpoints in the controller
[Authorize] //Require users to be authenticated (with a valid JWT token) to access any endpoint
public class UsersController : ControllerBase
{
    //ISender object from MediatR, used to send Queries and Commands
    //MediatR is a library that helps implement the CQRS pattern (Command Query Responsibility Segregation)
    private readonly ISender _sender;

    //Constructor - initialization function, receives ISender via dependency injection
    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// GET /api/v1/users/me
    /// Returns the full profile of the authenticated user (identified by JWT sub claim).
    ///Returns the complete profile of the authenticated user
    /// </summary>
    [HttpGet("me")] //Identify this as the GET endpoint at route /api/v1/users/me
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        //Send GetProfileQuery through MediatR to get the user profile
        //CancellationToken allows the task to be canceled if needed
        var result = await _sender.Send(new GetProfileQuery(), cancellationToken);

        //Returns HTTP 200 OK with user profile data
        return Ok(new
        {
            //Notification of success in English
            message = "Retrieved personal information successfully.",
            //User profile data
            data = result
        });
    }

    /// <summary>
    /// PUT /api/v1/users/me
    ///Updates phone number and address immediately - Updates phone number and address immediately
    /// If NewEmail is provided and different from current email, sends a verification
    /// email via Notification Service — the email is NOT changed until confirmed.
    ///If issued NewEmail and different from current email, send verification email - email cannot be changed until confirmed
    /// </summary>
    [HttpPut("me")] //Identify this as the PUT endpoint at route /api/v1/users/me
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request, //Get data from request body (JSON)
        CancellationToken cancellationToken) //Token allows asynchronous operation to be canceled
    {
        //Create an UpdateProfileCommand command with data from the request
        var command = new UpdateProfileCommand(
            PhoneNumber: request.PhoneNumber, //Update phone number
            NewEmail: request.NewEmail); //New email (if any)

        //Send commands through MediatR to handle profile updates
        var result = await _sender.Send(command, cancellationToken);

        //Returns HTTP 200 OK with updated results
        return Ok(new
        {
            //Notification from processing results
            message = result.Message,
            //The resulting data contains flags
            data = new
            {
                result.IsUpdated, //Flag indicating the profile has been updated
                result.EmailVerificationSent //Flag indicating the verification email was sent
            }
        });
    }

    /// <summary>
    /// POST /api/v1/users/me/phone/request-otp
    /// Sends an SMS OTP to the authenticated user's phone number to verify it.
    /// </summary>
    [HttpPost("me/phone/request-otp")]
    public async Task<IActionResult> RequestPhoneOtp(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RequestPhoneOtpCommand(), cancellationToken);
        return Ok(new { message = "OTP sent via SMS.", data = result });
    }

    /// <summary>
    /// POST /api/v1/users/me/phone/verify-otp
    /// Verifies the OTP code sent to the authenticated user's phone number.
    /// </summary>
    [HttpPost("me/phone/verify-otp")]
    public async Task<IActionResult> VerifyPhoneOtp(
    [FromBody] VerifyPhoneOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new VerifyPhoneOtpCommand(request.OtpCode), cancellationToken);
        return Ok(new { message = "Phone number verified successfully.", data = result });
    }

    [HttpPut("addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddess(Guid addressId,
        [FromBody] UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAddressCommand(
            addressId,
            request.RecipientName,
            request.RecipientPhone,
            request.Province,
            request.Ward,
            request.Street,
            request.Type,
            request.IsDefault);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(new { message = "Address updated successfully", data = response });
    }

    /// <summary>
    /// POST /api/v1/users/addresses
    /// Create a new address for the currently logged-in user.
    /// </summary>
    [HttpPost("addresses")]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAddressCommand
            (
                request.RecipientName,
                request.RecipientPhone,
                request.Province,
                request.Ward,
                request.Street,
                request.Type,
                request.IsDefault
            );
        var response = await _sender.Send(command, cancellationToken);
        // 201 Created
        return StatusCode(StatusCodes.Status201Created,
    new { message = "Address created successfully", data = response });
    }

    /// <summary>
    /// GET /api/v1/users/addresses
    /// Get a list of the addresses of the currently logged-in users.
    /// </summary>
    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAddressesQuery(), cancellationToken);
        return Ok(new { messase = "Retrieved addresses successfully", data = response });
    }

    /// <summary>
    /// GET /api/v1/users/addresses/{addressId}
    /// Get the details of the address of the currently logged-in user.
    /// </summary>
    [HttpGet("addresses/{addressId:guid}")]
    public async Task<IActionResult> GetAddressById(Guid addressId, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAddressByIdQuery(addressId), cancellationToken);
        return Ok(new { message = "Retrieved address successfully", data = response });
    }

    /// <summary>
    /// DELETE /api/v1/users/addresses/{addressId}
    /// Soft delete the address of the currently logged-in user.
    /// </summary
    [HttpDelete("addresses/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAddressCommand(addressId), cancellationToken);
        return Ok(new { message = "Address deleted successfully" });
    }

    /// <summary>
    /// PATCH /api/v1/users/addresses/{addressId}/default
    /// Set the default IP address for the logged-in user.
    /// </summary
    [HttpPatch("addresses/{addressId:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId, CancellationToken cancellationToken)
    {
        await _sender.Send(new SetDefaultAddressCommand(addressId), cancellationToken);
        return Ok(new { message = "Default address set succesfully" });
    }
}