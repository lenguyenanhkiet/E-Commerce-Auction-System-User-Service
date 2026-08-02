using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class OrderReputationFinalizedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<OrderReputationFinalized>
{
    public async Task Consume(ConsumeContext<OrderReputationFinalized> context)
    {
        var message = context.Message;
        var support = new ReputationConsumerSupport(mutations, unitOfWork, publisher);
        await support.ApplyAsync(
                message.MessageId, message.CorrelationId, message.OccurredAt,
                message.SourceService, nameof(OrderReputationFinalized),
                message.OrderId.ToString("N"), message.BuyerId, ReputationRoles.Buyer,
                context.CancellationToken,
                false,
                (ReputationReasonCatalog.BuyerOrderValue,
                    ReputationScoreCalculator.CalculateTransactionValue(message.TransactionAmount)),
                (ReputationReasonCatalog.BuyerFirstOrderCompleted,
                    message.IsFirstCompletedOrder ? 5 : 0));
        await support.ApplyAsync(
                message.MessageId, message.CorrelationId, message.OccurredAt,
                message.SourceService, nameof(OrderReputationFinalized),
                message.OrderId.ToString("N"), message.SellerId, ReputationRoles.Seller,
                context.CancellationToken,
                true,
                (ReputationReasonCatalog.SellerOrderCompleted, 5),
                (ReputationReasonCatalog.SellerShipmentOnTime, message.ShippedOnTime ? 3 : 0),
                (ReputationReasonCatalog.SellerItemAsDescribed, message.ItemAsDescribed ? 2 : 0),
                (ReputationReasonCatalog.SellerPositiveReview, message.PositiveVerifiedReview ? 2 : 0),
                (ReputationReasonCatalog.SellerHandledWithinSla, message.HandledWithinSla ? 1 : 0),
                (ReputationReasonCatalog.SellerHighValueOrder, message.IsHighValueOrder ? 2 : 0));
    }
}
