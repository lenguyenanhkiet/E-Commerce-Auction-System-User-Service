using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionSellerReputationFinalizedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<AuctionSellerReputationFinalized>
{
    public Task Consume(ConsumeContext<AuctionSellerReputationFinalized> context)
    {
        var m = context.Message;
        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(AuctionSellerReputationFinalized), m.AuctionId.ToString("N"),
            m.SellerId, ReputationRoles.Seller,
            context.CancellationToken,
            true,
            (ReputationReasonCatalog.SellerAuctionTransactionValue,
                ReputationScoreCalculator.CalculateTransactionValue(m.FinalPrice)),
            (ReputationReasonCatalog.SellerAuctionCommitmentBonus, m.CommitmentQualified ? 20 : 0),
            (ReputationReasonCatalog.SellerAuctionFirstCompleted, m.IsFirstCompletedAuction ? 10 : 0),
            (ReputationReasonCatalog.SellerAuctionShippedOnTime, m.ShippedOnTime ? 5 : 0),
            (ReputationReasonCatalog.SellerAuctionItemAsDescribed, m.ItemAsDescribed ? 5 : 0));
    }
}
