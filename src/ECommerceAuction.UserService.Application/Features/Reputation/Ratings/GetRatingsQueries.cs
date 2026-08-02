using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Ratings;

public sealed record GetGivenRatingsQuery(int Page = 1, int PageSize = 20) : IQuery<PagedRatingsResponse>;
public sealed record GetReceivedRatingsQuery(int Page = 1, int PageSize = 20) : IQuery<PagedRatingsResponse>;
public sealed record PagedRatingsResponse(IReadOnlyList<RatingResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public sealed class GetGivenRatingsQueryHandler(
    ICurrentUserService currentUser,
    IReputationRatingRepository repository) : IQueryHandler<GetGivenRatingsQuery, PagedRatingsResponse>
{
    public async Task<PagedRatingsResponse> Handle(GetGivenRatingsQuery request, CancellationToken cancellationToken)
    {
        Validate(request.Page, request.PageSize);
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var result = await repository.GetGivenRatingsAsync(userId, request.Page, request.PageSize, cancellationToken);
        return Map(result, request.Page, request.PageSize);
    }

    internal static void Validate(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1 and PageSize between 1 and 100.");
    }

    internal static PagedRatingsResponse Map((IReadOnlyList<UserRating> Items, int Total) result, int page, int pageSize) =>
        new(result.Items.Select(x => new RatingResponse(x.Id, x.TransactionType, x.TransactionId,
            x.RaterUserId, x.TargetUserId, x.Score, x.Comment, x.CreatedAt)).ToList(),
            page, pageSize, result.Total, result.Total == 0 ? 0 : (int)Math.Ceiling(result.Total / (double)pageSize));
}

public sealed class GetReceivedRatingsQueryHandler(
    ICurrentUserService currentUser,
    IReputationRatingRepository repository) : IQueryHandler<GetReceivedRatingsQuery, PagedRatingsResponse>
{
    public async Task<PagedRatingsResponse> Handle(GetReceivedRatingsQuery request, CancellationToken cancellationToken)
    {
        GetGivenRatingsQueryHandler.Validate(request.Page, request.PageSize);
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var result = await repository.GetReceivedRatingsAsync(userId, request.Page, request.PageSize, cancellationToken);
        return GetGivenRatingsQueryHandler.Map(result, request.Page, request.PageSize);
    }
}
