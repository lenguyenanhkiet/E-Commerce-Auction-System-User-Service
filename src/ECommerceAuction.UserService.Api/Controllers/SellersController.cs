using ECommerceAuction.UserService.Application.Features.Sellers.RegisterSeller;
using ECommerceAuction.UserService.Application.Features.Sellers.ResubmitSeller;
using ECommerceAuction.UserService.Application.Features.Sellers.GetMySellerProfile;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAuction.UserService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/sellers")]
    [Authorize]
    public class SellersController : ControllerBase
    {
        private readonly ISender _sender;
        public SellersController(ISender sender)
        {
            _sender = sender;
        }
        /// <summary>
        /// POST /api/v1/sellers/register — submit a new seller application.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterSellerRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RegisterSellerCommand(
                UserId: GetCurrentUserId(),
                SellerType: request.SellerType,
                BusinessName: request.BusinessName,
                TaxCode: request.TaxCode,
                BusinessLicenseUrl: request.BusinessLicenseUrl,
                Address: request.Address,
                BankAccountNumber: request.BankAccountNumber,
                BankName: request.BankName,
                BankAccountHolder: request.BankAccountHolder);

            var result = await _sender.Send(command, cancellationToken);

            return Ok(new
            {
                message = "Seller application submitted successfully.",
                data = result
            });
        }

        /// <summary>
        /// GET /api/v1/sellers/me — view the status of your own seller application.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetMySellerProfileQuery(GetCurrentUserId()), cancellationToken);

            if (result is null)
            {
                return Ok(new { message = "No seller application found.", data = (object?)null });
            }

            return Ok(new { message = "Retrieved seller application successfully.", data = result });
        }

        /// <summary>
        /// POST /api/v1/sellers/resubmit — resubmit after a Rejected application.
        /// </summary>
        [HttpPost("resubmit")]
        public async Task<IActionResult> Resubmit(
            [FromBody] ResubmitSellerRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ResubmitSellerCommand(
                UserId: GetCurrentUserId(),
                BusinessName: request.BusinessName,
                TaxCode: request.TaxCode,
                BusinessLicenseUrl: request.BusinessLicenseUrl,
                Address: request.Address,
                BankAccountNumber: request.BankAccountNumber,
                BankName: request.BankName,
                BankAccountHolder: request.BankAccountHolder);

            var result = await _sender.Send(command, cancellationToken);

            return Ok(new { message = "Seller application resubmitted successfully.", data = result });
        }

        // ASSUMPTION: the user id claim type used across the project. RegisterAccountCommandHandler
        // creates the user id server-side (pending-registration flow) so it didn't show me how a
        // logged-in request resolves "current user id" from the JWT. ClaimTypes.NameIdentifier is
        // the ASP.NET default — swap for whatever custom claim (e.g. "uid", "sub") your JWT issuer
        // actually sets, if different.
        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }
    }

    public sealed record RegisterSellerRequest(
        string SellerType,
        string? BusinessName,
        string? TaxCode,
        string? BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder);

    public sealed record ResubmitSellerRequest(
        string? BusinessName,
        string? TaxCode,
        string? BusinessLicenseUrl,
        string Address,
        string BankAccountNumber,
        string BankName,
        string BankAccountHolder);
}
