using ECommerceAuction.UserService.Application.Authorization;
using ECommerceAuction.UserService.Application.Features.Reputation.Admin;
using ECommerceAuction.UserService.Application.Features.Reputation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/admin/reputation")]
public sealed class AdminReputationController(ISender sender) : ControllerBase
{
    [Authorize(Policy = Permissions.ReputationAdmin.View)]
    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUser(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetAdminReputationQuery(userId, page, pageSize), cancellationToken));

    [Authorize(Policy = Permissions.ReputationAdmin.Adjust)]
    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustReputationCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ReputationAdmin.Reverse)]
    [HttpPost("entries/{entryId:guid}/reverse")]
    public async Task<IActionResult> Reverse(
        Guid entryId, [FromBody] ReverseReputationRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ReverseReputationEntryCommand(
            request.OperationId, entryId, request.UserId, request.EvidenceReference), cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ReputationAdmin.ClearRestriction)]
    [HttpPost("restrictions/clear")]
    public async Task<IActionResult> Clear(
        [FromBody] ClearReputationRestrictionCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }
}

public sealed record ReverseReputationRequest(
    Guid OperationId, Guid UserId, string EvidenceReference);
