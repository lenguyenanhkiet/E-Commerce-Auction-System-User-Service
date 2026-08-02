using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Ratings;

public sealed record SubmitRatingCommand(
    string TransactionType,
    Guid TransactionId,
    Guid TargetUserId,
    int Score,
    string? Comment) : ICommand<RatingResponse>;

public sealed record RatingResponse(
    Guid Id, string TransactionType, Guid TransactionId,
    Guid RaterUserId, Guid TargetUserId, int Score,
    string? Comment, DateTimeOffset CreatedAt);
