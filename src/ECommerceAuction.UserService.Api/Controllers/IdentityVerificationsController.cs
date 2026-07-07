using ECommerceAuction.UserService.Application.Features.Identities.GetMyIdentityVerification;
using ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAuction.UserService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/identity-verifications")]
    [Authorize]
    public class IdentityVerificationsController : ControllerBase
    {
        private readonly ISender _sender;
        public IdentityVerificationsController(ISender sender)
        {
            _sender = sender;
        }
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitIdentityVerificationRequest request, CancellationToken cancellationToken)
        {
            var command = new SubmitIdentityVerificationCommand(
                UserId: GetCurrentUserId(),
                IdentityNumber: request.IdentityNumber,
                FrontImageUrl: request.FrontImageUrl,
                BackImageUrl: request.BackImageUrl);

            var response = await _sender.Send(command, cancellationToken);
            return Ok(new
            {
                message = response.status == "Verified"
                ? "Identity verified successfully."
                : "Identity verification failed.",
                data = response
            });
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetMyIdentityVerificationQuery(GetCurrentUserId()), cancellationToken);

            if (result is null)
            {
                return Ok(new { message = "No identity verification found.", data = (object?)null });
            }

            return Ok(new { message = "Retrieved identity verification successfully.", data = result });
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }
    }

    public sealed record SubmitIdentityVerificationRequest(
        string IdentityNumber,
        string FrontImageUrl,
        string BackImageUrl);
}
