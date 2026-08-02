using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class ProductReviewRevokedConsumer(
    IReputationMutationService mutations,
    IReputationLedgerRepository ledger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<ProductReviewRevoked>
{
    public async Task Consume(ConsumeContext<ProductReviewRevoked> context)
    {
        var m = context.Message;
        var original = await ledger.GetBySourceAsync(
            m.ReviewId.ToString("N"),
            m.ReasonCode,
            context.CancellationToken)
            ?? throw new InvalidOperationException("Original review reputation entry is not available yet.");

        await new ReputationConsumerSupport(mutations, unitOfWork, publisher).ReverseAsync(
            original.Id, m.MessageId, m.CorrelationId, m.OccurredAt,
            m.BuyerId,
            m.ReviewId.ToString("N"), context.CancellationToken);
    }
}
