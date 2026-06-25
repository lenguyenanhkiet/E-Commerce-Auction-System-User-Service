//Enter the necessary namespaces
using ECommerceAuction.UserService.Application.Features.Admin.Users.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
///Admin-only user management endpoints - User management endpoints for admin only
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/v1/admin/users")] //Defines a root route for all endpoints in the controller
[Authorize(Roles = "ADMIN")] //Requires users to have the "Admin" role to access
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
}