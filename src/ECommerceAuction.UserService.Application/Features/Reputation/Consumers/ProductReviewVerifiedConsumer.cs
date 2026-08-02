using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class ProductReviewVerifiedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<ProductReviewVerified>
{
    public Task Consume(ConsumeContext<ProductReviewVerified> context)
    {
        var m = context.Message;
        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(ProductReviewVerified), m.ReviewId.ToString("N"),
            m.BuyerId, ReputationRoles.Buyer,
            context.CancellationToken,
            true,
            (m.HasMedia
                    ? ReputationReasonCatalog.BuyerReviewWithMedia
                    : ReputationReasonCatalog.BuyerReviewSubmitted,
                ReputationScoreCalculator.CalculateProductReviewScore(m.HasMedia)));
    }
}
