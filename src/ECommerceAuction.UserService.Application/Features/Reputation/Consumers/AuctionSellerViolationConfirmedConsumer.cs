using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionSellerViolationConfirmedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<AuctionSellerViolationConfirmed>
{
    private static readonly IReadOnlyDictionary<string, int> Fixed =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [ReputationReasonCatalog.SellerAuctionCancelledAfterBid] = -100,
            [ReputationReasonCatalog.SellerAuctionCancelledNearEnd] = -200,
            [ReputationReasonCatalog.SellerAuctionRefusedWinningPrice] = -300,
            [ReputationReasonCatalog.SellerAuctionShipmentLateMinor] = -10,
            [ReputationReasonCatalog.SellerAuctionShipmentLateMajor] = -30,
            [ReputationReasonCatalog.SellerAuctionShipmentLateSevere] = -60,
            [ReputationReasonCatalog.SellerAuctionMismatchMinor] = -30,
            [ReputationReasonCatalog.SellerAuctionMismatchMajor] = -150,
            [ReputationReasonCatalog.SellerAuctionMismatchSevere] = -300,
            [ReputationReasonCatalog.SellerAuctionCounterfeit] = -500,
            [ReputationReasonCatalog.SellerAuctionFakeTracking] = -150,
            [ReputationReasonCatalog.SellerAuctionFakeTrackingSevere] = -300,
            [ReputationReasonCatalog.SellerAuctionShillBiddingConfirmed] = -500
        };

    public Task Consume(ConsumeContext<AuctionSellerViolationConfirmed> context)
    {
        var m = context.Message;
        var delta = m.ReasonCode == ReputationReasonCatalog.SellerAuctionNotFulfilled
            ? ReputationScoreCalculator.CalculateAuctionNonFulfillmentPenalty(m.FinalPrice)
            : Fixed.TryGetValue(m.ReasonCode, out var penalty)
                ? penalty
                : throw new ArgumentException($"Unsupported auction violation '{m.ReasonCode}'.");

        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(AuctionSellerViolationConfirmed), m.AuctionId.ToString("N"),
            m.SellerId, ReputationRoles.Seller, context.CancellationToken, true,
            (m.ReasonCode, delta));
    }
}
