using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            message = "Backend connected successfully!",
            project = "ECommerceAuction.UserService",
            time = DateTime.Now
        });
    }
}
