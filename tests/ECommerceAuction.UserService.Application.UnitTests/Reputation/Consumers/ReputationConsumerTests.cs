using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Consumers;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Consumers;

public sealed class ReputationConsumerTests
{
    [Fact]
    public async Task Verified_review_with_media_awards_five_points_and_saves_once()
    {
        var mutationService = Substitute.For<IReputationMutationService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var publisher = Substitute.For<IPublishEndpoint>();
        var context = Substitute.For<ConsumeContext<ProductReviewVerified>>();
        var message = new ProductReviewVerified(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "commerce-service",
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true,
            ReputationReasonCatalog.BuyerReviewWithMedia);
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        mutationService.ApplyAsync(
                Arg.Any<ReputationMutation>(),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.DuplicateResult());

        await new ProductReviewVerifiedConsumer(
                mutationService, unitOfWork, publisher)
            .Consume(context);

        await mutationService.Received(1).ApplyAsync(
            Arg.Is<ReputationMutation>(mutation =>
                mutation.UserId == message.BuyerId &&
                mutation.Role == ReputationRoles.Buyer &&
                mutation.ReasonCode == ReputationReasonCatalog.BuyerReviewWithMedia &&
                mutation.ScoreDelta == 5 &&
                mutation.SourceId == message.ReviewId.ToString("N")),
            CancellationToken.None);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
        await publisher.DidNotReceiveWithAnyArgs().Publish(
            default(ReputationScoreUpdated)!,
            default);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 0)]
    public async Task Auction_participation_is_limited_to_first_five_qualified_events(
        int sequence,
        int expectedMutationCalls)
    {
        var mutationService = Substitute.For<IReputationMutationService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var publisher = Substitute.For<IPublishEndpoint>();
        var context = Substitute.For<ConsumeContext<AuctionParticipationQualified>>();
        context.Message.Returns(new AuctionParticipationQualified(
            Guid.NewGuid(), null, DateTimeOffset.UtcNow, "auction-service",
            Guid.NewGuid(), Guid.NewGuid(), sequence,
            ReputationReasonCatalog.BuyerAuctionActiveParticipation));
        context.CancellationToken.Returns(CancellationToken.None);
        mutationService.ApplyAsync(
                Arg.Any<ReputationMutation>(),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.DuplicateResult());

        await new AuctionParticipationQualifiedConsumer(
                mutationService, unitOfWork, publisher)
            .Consume(context);

        await mutationService.Received(expectedMutationCalls).ApplyAsync(
            Arg.Any<ReputationMutation>(),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Buyer_payment_default_applies_fixed_minus_three_hundred()
    {
        var mutationService = Substitute.For<IReputationMutationService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var publisher = Substitute.For<IPublishEndpoint>();
        var context = Substitute.For<ConsumeContext<AuctionBuyerPaymentDefaulted>>();
        context.Message.Returns(new AuctionBuyerPaymentDefaulted(
            Guid.NewGuid(), null, DateTimeOffset.UtcNow, "auction-service",
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2_000_000m,
            ReputationReasonCatalog.BuyerAuctionWinnerPaymentDefault, "CONFIRMED"));
        context.CancellationToken.Returns(CancellationToken.None);
        mutationService.ApplyAsync(
                Arg.Any<ReputationMutation>(),
                Arg.Any<CancellationToken>())
            .Returns(ReputationMutationResult.DuplicateResult());

        await new AuctionBuyerPaymentDefaultedConsumer(
                mutationService, unitOfWork, publisher)
            .Consume(context);

        await mutationService.Received(1).ApplyAsync(
            Arg.Is<ReputationMutation>(mutation => mutation.ScoreDelta == -300),
            CancellationToken.None);
    }
}
