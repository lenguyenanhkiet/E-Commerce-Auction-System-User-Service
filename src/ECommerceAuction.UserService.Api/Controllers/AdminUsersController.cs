//Enter the necessary namespaces
using ECommerceAuction.UserService.Api.Authorization;
using ECommerceAuction.UserService.Application.Features.Admin.Users.ChangeUserPassword;
using ECommerceAuction.UserService.Application.Features.Admin.Users.CreateUser;
using ECommerceAuction.UserService.Application.Features.Admin.Users.DeleteUser;
using ECommerceAuction.UserService.Application.Features.Admin.Users.GetUserById;
using ECommerceAuction.UserService.Application.Features.Admin.Users.GetUsers;
using ECommerceAuction.UserService.Application.Features.Admin.Users.UpdateUser;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
///Admin user management endpoints, protected by fine-grained RBAC privileges.
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/v1/admin/users")] //Defines a root route for all endpoints in the controller
[Authorize] //Require an authenticated user; each endpoint enforces its own privilege
public class AdminUsersController : ControllerBase
{
    //ISender object from MediatR, used to send Queries and Commands
    private readonly ISender _sender;

    //Constructor - initialization function, receives ISender via dependency injection
    public AdminUsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    ///GET /api/v1/admin/users - Get a list of all users
    ///
    ///Query parameters:
    ///search – search by FullName, Email, PhoneNumber (optional)
    ///gender – filter by gender (optional)
    ///status – filter by status (optional)
    ///page – page number based on 1 (default 1)
    ///pageSize – number of items per page, maximum 100 (default 20)
    ///
    ///Default sort: FullName ASC (Default sort by FullName from A to Z)
    /// </summary>
    [HttpGet] //Identify this as the GET endpoint at route /api/v1/admin/users
    [RequirePrivilege(PrivilegeCodes.UserList)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search, //Search keyword (null if not available)
        [FromQuery] string? gender, //Filter by gender (null if not filtered)
        [FromQuery] string? status, //Filter by user status (null if not filtered)
        [FromQuery] int page = 1, //Current page number (default 1)
        [FromQuery] int pageSize = 20, //Number of users per page (default 20)
        CancellationToken cancellationToken = default) //Token allows asynchronous operation to be canceled
    {
        //Create a GetUsersQuery query with filtering and pagination parameters
        var query = new GetUsersQuery(
            Search: search, //Submit search keywords
            Gender: gender, //Submit gender filter
            Status: status, //Send status filter
            Page: page, //Submit page number
            PageSize: pageSize); //Submit page size

        //Send query through MediatR to get list of users
        var result = await _sender.Send(query, cancellationToken);

        //Returns HTTP 200 OK with list of users
        return Ok(new
        {

            message = "Retrieved user list successfully.",
            //User list data and pagination information
            data = result
        });
    }

    /// <summary>
    /// GET /api/v1/admin/users/{id} — view the detail of a single user.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePrivilege(PrivilegeCodes.UserView)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);

        return Ok(new
        {
            message = "Retrieved user successfully.",
            data = result
        });
    }

    /// <summary>
    /// POST /api/v1/admin/users — create a new user account.
    /// </summary>
    [HttpPost]
    [RequirePrivilege(PrivilegeCodes.UserCreate)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Id },
            new
            {
                message = "User created successfully.",
                data = result
            });
    }

    /// <summary>
    /// PUT /api/v1/admin/users/{id} — update an existing user's information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePrivilege(PrivilegeCodes.UserUpdate)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            UserId: id,
            FullName: request.FullName,
            Email: request.Email,
            Gender: request.Gender,
            DateOfBirth: request.DateOfBirth,
            Address: request.Address,
            RoleCodes: request.RoleCodes);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "User updated successfully.",
            data = result
        });
    }

    /// <summary>
    /// DELETE /api/v1/admin/users/{id} — soft-delete a user account.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePrivilege(PrivilegeCodes.UserDelete)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// POST /api/v1/admin/users/{id}/password — set a user's password.
    /// </summary>
    [HttpPost("{id:guid}/password")]
    [RequirePrivilege(PrivilegeCodes.UserChangePassword)]
    public async Task<IActionResult> ChangeUserPassword(
        Guid id,
        [FromBody] ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserPasswordCommand(
            UserId: id,
            NewPassword: request.NewPassword,
            RequireChangeOnNextLogin: request.RequireChangeOnNextLogin);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            message = "User password changed successfully.",
            data = result
        });
    }
}

/// <summary>
/// Request body for PUT /api/v1/admin/users/{id}.
/// Omit <see cref="RoleCodes"/> (null) to leave roles unchanged; supply a list to reconcile them.
/// </summary>
public sealed record UpdateUserRequest(
    string FullName,
    string? Email,
    string? Gender,
    DateOnly? DateOfBirth,
    string? Address,
    IReadOnlyList<string>? RoleCodes = null);

/// <summary>
/// Request body for POST /api/v1/admin/users/{id}/password.
/// </summary>
public sealed record ChangeUserPasswordRequest(
    string NewPassword,
    bool RequireChangeOnNextLogin = true);