using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionRatingEligibilityConsumer(
    IReputationRatingRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<AuctionSellerReputationFinalized>
{
    public async Task Consume(ConsumeContext<AuctionSellerReputationFinalized> context)
    {
        var m = context.Message;
        await AddIfMissing(m.AuctionId, m.BuyerId, m.SellerId, m.OccurredAt, context.CancellationToken);
        await AddIfMissing(m.AuctionId, m.SellerId, m.BuyerId, m.OccurredAt, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private async Task AddIfMissing(Guid auctionId, Guid rater, Guid target,
        DateTimeOffset opensAt, CancellationToken cancellationToken)
    {
        if (rater == Guid.Empty || target == Guid.Empty || rater == target) return;
        if (await repository.GetEligibilityAsync("AUCTION", auctionId, rater, target, cancellationToken) is not null) return;
        await repository.AddEligibilityAsync(
            TransactionRatingEligibility.Create("AUCTION", auctionId, rater, target, opensAt, opensAt.AddDays(30)),
            cancellationToken);
    }
}
