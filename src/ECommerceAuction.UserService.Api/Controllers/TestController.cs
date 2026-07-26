//Enter the required namespace
using Microsoft.AspNetCore.Mvc;

//Define namespace for this controller
namespace ECommerceAuction.UserService.Api.Controllers;

/// <summary>
///Test controller - controller tests backend connection
///Used to check if the backend connects successfully
/// </summary>
[ApiController] //Specify this as an API controller
[Route("api/test")] //Defines a root route for all endpoints in the controller
public class TestController : ControllerBase
{
    /// <summary>
    ///GET /api/test/ping - Test backend connection
    ///Simple endpoint to test to see if the backend is working
    /// </summary>
    [HttpGet("ping")] //Identify this as the GET endpoint at route /api/test/ping
    public IActionResult Ping()
    {
        //Returns HTTP 200 OK with a successful connection message
        return Ok(new
        {
            message = "Backend connected successfully!", //Notification of successful connection
            project = "ECommerceAuction.UserService", //Project name
            time = DateTimeOffset.Now //Current time
        });
    }
}
