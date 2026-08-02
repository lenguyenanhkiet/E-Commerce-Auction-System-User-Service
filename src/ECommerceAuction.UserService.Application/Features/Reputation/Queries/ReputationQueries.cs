using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using ECommerceAuction.UserService.Domain.Reputation.Seller;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Queries;

public sealed record ReputationProfileView(
    int Score, string TrustLevel, string? SellingRestrictionStatus,
    string AuctionRestrictionStatus, DateTimeOffset? RestrictedUntil,
    bool RequiresManualReview);
public sealed record RatingSummary(int Count, double Average);
public sealed record PersonalReputationResponse(
    ReputationProfileView? Buyer, ReputationProfileView? Seller,
    RatingSummary Ratings);
public sealed record PublicReputationResponse(
    Guid UserId, int? BuyerScore, string? BuyerTrustLevel,
    int? SellerScore, string? SellerTrustLevel, RatingSummary VerifiedRatings);
public sealed record LedgerItem(
    Guid Id, Guid UserId, string Role, string ReasonCode, int ScoreDelta,
    int ScoreBefore, int ScoreAfter, DateTimeOffset OccurredAt,
    Guid? ReversesEntryId, string? SourceService = null, string? SourceType = null,
    string? SourceId = null, string? EvidenceReference = null);
public sealed record PagedLedgerResponse(IReadOnlyList<LedgerItem> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed record GetMyReputationQuery : IQuery<PersonalReputationResponse>;
public sealed record GetPublicReputationQuery(Guid UserId) : IQuery<PublicReputationResponse>;
public sealed record GetReputationLedgerQuery(int Page = 1, int PageSize = 20) : IQuery<PagedLedgerResponse>;
public sealed record GetAdminReputationQuery(Guid UserId, int Page = 1, int PageSize = 20) : IQuery<(PersonalReputationResponse Profile, PagedLedgerResponse Ledger)>;

public sealed class ReputationQueryHandler(
    ICurrentUserService currentUser,
    IBuyerReputationRepository buyers,
    ISellerReputationRepository sellers,
    IReputationRatingRepository ratings,
    IReputationLedgerRepository ledger) :
    IQueryHandler<GetMyReputationQuery, PersonalReputationResponse>,
    IQueryHandler<GetPublicReputationQuery, PublicReputationResponse>,
    IQueryHandler<GetReputationLedgerQuery, PagedLedgerResponse>,
    IQueryHandler<GetAdminReputationQuery, (PersonalReputationResponse Profile, PagedLedgerResponse Ledger)>
{
    public Task<PersonalReputationResponse> Handle(GetMyReputationQuery request, CancellationToken cancellationToken) =>
        GetPersonalAsync(currentUser.UserId ?? throw new UnauthorizedAccessException(), cancellationToken);

    public async Task<PublicReputationResponse> Handle(GetPublicReputationQuery request, CancellationToken cancellationToken)
    {
        var buyer = await buyers.GetByUserIdAsync(request.UserId, cancellationToken);
        var seller = await sellers.GetByUserIdAsync(request.UserId, cancellationToken);
        if (buyer is null && seller is null) throw new NotFoundException("Reputation profile was not found.");
        var summary = await ratings.GetReceivedSummaryAsync(request.UserId, cancellationToken);
        return new PublicReputationResponse(request.UserId, buyer?.ConfirmedScore, buyer?.TrustLevel,
            seller?.ConfirmedScore, seller?.TrustLevel, new RatingSummary(summary.Count, summary.Average));
    }

    public Task<PagedLedgerResponse> Handle(GetReputationLedgerQuery request, CancellationToken cancellationToken) =>
        GetLedgerAsync(currentUser.UserId ?? throw new UnauthorizedAccessException(), request.Page, request.PageSize, false, cancellationToken);

    public async Task<(PersonalReputationResponse Profile, PagedLedgerResponse Ledger)> Handle(
        GetAdminReputationQuery request, CancellationToken cancellationToken) =>
        (await GetPersonalAsync(request.UserId, cancellationToken),
         await GetLedgerAsync(request.UserId, request.Page, request.PageSize, true, cancellationToken));

    private async Task<PersonalReputationResponse> GetPersonalAsync(Guid userId, CancellationToken cancellationToken)
    {
        var buyer = await buyers.GetByUserIdAsync(userId, cancellationToken);
        var seller = await sellers.GetByUserIdAsync(userId, cancellationToken);
        var summary = await ratings.GetReceivedSummaryAsync(userId, cancellationToken);
        return new PersonalReputationResponse(
            buyer is null ? null : new ReputationProfileView(buyer.ConfirmedScore, buyer.TrustLevel, null,
                buyer.AuctionRestrictionStatus, buyer.RestrictedUntil, buyer.RequiresManualReview),
            seller is null ? null : new ReputationProfileView(seller.ConfirmedScore, seller.TrustLevel,
                seller.SellingRestrictionStatus, seller.AuctionRestrictionStatus,
                seller.RestrictedUntil, seller.RequiresManualReview),
            new RatingSummary(summary.Count, summary.Average));
    }

    private async Task<PagedLedgerResponse> GetLedgerAsync(Guid userId, int page, int pageSize, bool admin, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(page));
        var result = await ledger.GetPagedAsync(userId, page, pageSize, cancellationToken);
        var items = result.Items.Select(x => new LedgerItem(x.Id, x.UserId, x.Role, x.ReasonCode,
            x.ScoreDelta, x.ScoreBefore, x.ScoreAfter, x.OccurredAt, x.ReversesEntryId,
            admin ? x.SourceService : null, admin ? x.SourceType : null,
            admin ? x.SourceId : null, admin ? x.EvidenceReference : null)).ToList();
        return new PagedLedgerResponse(items, page, pageSize, result.Total,
            result.Total == 0 ? 0 : (int)Math.Ceiling(result.Total / (double)pageSize));
    }
}
