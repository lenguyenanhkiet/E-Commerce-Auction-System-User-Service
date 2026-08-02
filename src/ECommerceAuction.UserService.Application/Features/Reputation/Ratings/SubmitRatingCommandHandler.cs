using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Ratings;

public sealed class SubmitRatingCommandHandler(
    ICurrentUserService currentUser,
    IReputationRatingRepository repository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    TimeProvider timeProvider) : ICommandHandler<SubmitRatingCommand, RatingResponse>
{
    public async Task<RatingResponse> Handle(SubmitRatingCommand request, CancellationToken cancellationToken)
    {
        var raterId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        if (raterId == request.TargetUserId) throw new InvalidOperationException("Self-rating is not allowed.");
        var type = request.TransactionType.Trim().ToUpperInvariant();
        var eligibility = await repository.GetEligibilityAsync(
            type, request.TransactionId, raterId, request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException("Rating eligibility was not found.");
        if (await repository.ExistsRatingAsync(
                type, request.TransactionId, raterId, request.TargetUserId, cancellationToken))
            throw new ConflictException("Rating was already submitted.");

        var now = timeProvider.GetUtcNow();
        var rating = UserRating.Create(
            type, request.TransactionId, raterId, request.TargetUserId,
            request.Score, request.Comment, now);
        eligibility.MarkSubmitted(rating.Id, now);
        await repository.AddRatingAsync(rating, cancellationToken);
        await publisher.Publish(new UserRated(
            Guid.NewGuid(), null, now, rating.Id, type, request.TransactionId,
            raterId, request.TargetUserId, request.Score), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RatingResponse(
            rating.Id, rating.TransactionType, rating.TransactionId,
            rating.RaterUserId, rating.TargetUserId, rating.Score,
            rating.Comment, rating.CreatedAt);
    }
}
