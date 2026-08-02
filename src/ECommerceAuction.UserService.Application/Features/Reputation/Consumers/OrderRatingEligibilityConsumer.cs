using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class OrderRatingEligibilityConsumer(
    IReputationRatingRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<OrderReputationFinalized>
{
    public async Task Consume(ConsumeContext<OrderReputationFinalized> context)
    {
        var m = context.Message;
        await AddIfMissing("ORDER", m.OrderId, m.BuyerId, m.SellerId, m.OccurredAt, context.CancellationToken);
        await AddIfMissing("ORDER", m.OrderId, m.SellerId, m.BuyerId, m.OccurredAt, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private async Task AddIfMissing(string type, Guid transactionId, Guid rater, Guid target,
        DateTimeOffset opensAt, CancellationToken cancellationToken)
    {
        if (rater == Guid.Empty || target == Guid.Empty || rater == target) return;
        if (await repository.GetEligibilityAsync(type, transactionId, rater, target, cancellationToken) is not null) return;
        await repository.AddEligibilityAsync(
            TransactionRatingEligibility.Create(type, transactionId, rater, target, opensAt, opensAt.AddDays(30)),
            cancellationToken);
    }
}
