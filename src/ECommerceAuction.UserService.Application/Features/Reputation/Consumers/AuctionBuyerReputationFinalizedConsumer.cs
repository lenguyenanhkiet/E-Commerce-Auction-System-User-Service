using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionBuyerReputationFinalizedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<AuctionBuyerReputationFinalized>
{
    public Task Consume(ConsumeContext<AuctionBuyerReputationFinalized> context)
    {
        var m = context.Message;
        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(AuctionBuyerReputationFinalized), m.AuctionId.ToString("N"),
            m.BuyerId, ReputationRoles.Buyer,
            context.CancellationToken,
            true,
            (ReputationReasonCatalog.BuyerAuctionTransactionValue,
                m.WinnerPaid ? ReputationScoreCalculator.CalculateTransactionValue(m.FinalPrice) : 0),
            (ReputationReasonCatalog.BuyerAuctionCommitmentBonus,
                m.CommitmentQualified ? 20 : 0));
    }
}
