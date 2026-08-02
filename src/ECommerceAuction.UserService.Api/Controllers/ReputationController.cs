using ECommerceAuction.UserService.Application.Features.Reputation.Ratings;
using ECommerceAuction.UserService.Application.Features.Reputation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAuction.UserService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/reputation")]
public sealed class ReputationController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<PersonalReputationResponse>> GetMine(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetMyReputationQuery(), cancellationToken));

    [AllowAnonymous]
    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<PublicReputationResponse>> GetPublic(
        Guid userId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetPublicReputationQuery(userId), cancellationToken));

    [HttpGet("ledger")]
    public async Task<ActionResult<PagedLedgerResponse>> GetLedger(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetReputationLedgerQuery(page, pageSize), cancellationToken));

    [HttpPost("ratings")]
    public async Task<ActionResult<RatingResponse>> SubmitRating(
        [FromBody] SubmitRatingCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command, cancellationToken));

    [HttpGet("ratings/given")]
    public async Task<ActionResult<PagedRatingsResponse>> GetGivenRatings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetGivenRatingsQuery(page, pageSize), cancellationToken));

    [HttpGet("ratings/received")]
    public async Task<ActionResult<PagedRatingsResponse>> GetReceivedRatings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetReceivedRatingsQuery(page, pageSize), cancellationToken));
}
