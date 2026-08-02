using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class AuctionParticipationQualifiedConsumer(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<AuctionParticipationQualified>
{
    public Task Consume(ConsumeContext<AuctionParticipationQualified> context)
    {
        var m = context.Message;
        var delta = m.QualifiedSequenceForDay is >= 1 and <= 5 ? 1 : 0;
        return new ReputationConsumerSupport(mutations, unitOfWork, publisher).ApplyAsync(
            m.MessageId, m.CorrelationId, m.OccurredAt, m.SourceService,
            nameof(AuctionParticipationQualified), m.AuctionId.ToString("N"),
            m.BuyerId, ReputationRoles.Buyer,
            context.CancellationToken,
            true,
            (ReputationReasonCatalog.BuyerAuctionActiveParticipation, delta));
    }
}
