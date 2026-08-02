using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

public sealed class ReputationDecisionConsumer(
    IReputationMutationService mutations,
    IReputationLedgerRepository ledger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher) : IConsumer<ReputationDecision>
{
    public async Task Consume(ConsumeContext<ReputationDecision> context)
    {
        var m = context.Message;
        if (!m.DecisionType.Equals("REVERSE", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Unsupported reputation decision '{m.DecisionType}'.");

        var original = await ledger.GetByIdempotencyKeyAsync(
            m.OriginalIdempotencyKey,
            context.CancellationToken)
            ?? throw new InvalidOperationException("Original reputation entry is not available yet.");

        await new ReputationConsumerSupport(mutations, unitOfWork, publisher).ReverseAsync(
            original.Id, m.MessageId, m.CorrelationId, m.OccurredAt,
            m.UserId,
            m.EvidenceReference, context.CancellationToken);
    }
}
