using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthCheckController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new
        {
            status = "Healthy",
            service = "ECommerceAuction.UserService.Api",
            timestampUtc = DateTime.UtcNow
        };

        return Task.FromResult<IActionResult>(Ok(response));
    }
}

