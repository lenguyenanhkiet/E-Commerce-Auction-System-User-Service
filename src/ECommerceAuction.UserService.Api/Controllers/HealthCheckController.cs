//Enter the required namespace
using Microsoft.AspNetCore.Mvc;

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
///Health check controller - checks the health status of the service
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/health")] //Defines a root route for all endpoints in the controller
public sealed class HealthCheckController : ControllerBase
{
    /// <summary>
    ///GET /api/health - Get the health status of the service
    ///This endpoint is used to check if the service is running normally
    /// </summary>
    [HttpGet] //Identify this as the GET endpoint at route /api/health
    public Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        //Check if the request has been aborted, if so, throw an exception
        cancellationToken.ThrowIfCancellationRequested();

        //Create a response object containing service status information
        var response = new
        {
            status = "Healthy", //Health status: Healthy (healthy)
            service = "ECommerceAuction.UserService.Api", //Service name
            timestampUtc = DateTime.UtcNow //Current time in UTC
        };

        //Returns HTTP 200 OK with response object
        return Task.FromResult<IActionResult>(Ok(response));
    }
}

