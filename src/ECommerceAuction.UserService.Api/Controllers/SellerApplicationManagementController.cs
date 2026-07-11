using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Application.Features.Admin.Sellers.ApproveSeller;
using ECommerceAuction.UserService.Application.Features.Admin.Sellers.GetSellerApplicationById;
using ECommerceAuction.UserService.Application.Features.Admin.Sellers.GetSellerApplications;
using ECommerceAuction.UserService.Application.Features.Admin.Sellers.RejectSeller;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers
{
    /// <summary>
    /// Seller Application: Admin and Sp Staff can approve, reject application to become a seller
    /// </summary>
    [ApiController]
    [Route("api/v1/management/sellers-applications")]
    [Authorize]
    public class SellerApplicationManagementController : ControllerBase
    {
        private readonly ISender _sender;

        public SellerApplicationManagementController(ISender sender)
        {
            _sender = sender;
        }
        /// <summary>
        /// GET /api/v1/admin/sellers — list applications, optionally filtered by status.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Sellers.ListApplications)]
        public async Task<IActionResult> GetSellerApplications(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        {
            var query = new GetSellerApplicationsQuery(status, page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            return Ok(new
            {
                message = "Retrieved seller application list successfully.",
                data = result
            });
        }
        /// <summary>
        /// GET /api/v1/admin/sellers/{id} — view full detail of one application.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = Permissions.Sellers.ViewApplication)]
        public async Task<IActionResult> GetSellerApplicationById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetSellerApplicationByIdQuery(id), cancellationToken);

            return Ok(new { message = "Retrieved seller application successfully.", data = result });
        }
        /// <summary>
        /// PUT /api/v1/admin/sellers/{id}/approve
        /// </summary>
        [HttpPut("{id:guid}/approve")]
        [Authorize(Policy = Permissions.Sellers.ApproveApplication)]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var adminUserId = GetCurrentUserId();
            var result = await _sender.Send(new ApproveSellerCommand(id, adminUserId), cancellationToken);

            return Ok(new { message = "Seller application approved.", data = result });
        }

        /// <summary>
        /// PUT /api/v1/admin/sellers/{id}/reject
        /// </summary>
        [HttpPut("{id:guid}/reject")]
        [Authorize(Policy = Permissions.Sellers.RejectApplication)]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectSellerRequest request,
            CancellationToken cancellationToken)
        {
            var adminUserId = GetCurrentUserId();
            var result = await _sender.Send(new RejectSellerCommand(id, adminUserId, request.Reason), cancellationToken);

            return Ok(new { message = "Seller application rejected.", data = result });
        }

        // ASSUMPTION: see the same note in SellersController.GetCurrentUserId — confirm the claim type.
        private Guid GetCurrentUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }
    }
    public sealed record RejectSellerRequest(string Reason);

}
