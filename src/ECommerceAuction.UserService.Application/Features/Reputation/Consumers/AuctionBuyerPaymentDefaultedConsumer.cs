using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionBuyerPaymentDefaultedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<AuctionBuyerPaymentDefaulted>
{
    public Task Consume(ConsumeContext<AuctionBuyerPaymentDefaulted> context)
    {
        var m = context.Message;
        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(AuctionBuyerPaymentDefaulted), m.AuctionId.ToString("N"),
            m.BuyerId, ReputationRoles.Buyer, context.CancellationToken, true,
            (ReputationReasonCatalog.BuyerAuctionWinnerPaymentDefault, -300));
    }
}
