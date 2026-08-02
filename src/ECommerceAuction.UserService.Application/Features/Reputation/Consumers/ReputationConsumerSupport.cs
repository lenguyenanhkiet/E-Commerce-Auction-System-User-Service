using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using MassTransit;
using Nexus.Contracts.Events.Reputation.V1;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Consumers;

internal sealed class ReputationConsumerSupport(
    IReputationMutationService mutations,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher)
{
    public async Task ApplyAsync(
        Guid messageId,
        Guid? correlationId,
        DateTimeOffset occurredAt,
        string sourceService,
        string sourceType,
        string sourceId,
        Guid userId,
        string role,
        CancellationToken cancellationToken,
        bool saveChanges = true,
        params (string Reason, int Delta)[] changes)
    {
        Validate(messageId, occurredAt, sourceService, sourceId, userId);
        var applied = new List<(ReputationMutationResult Result, string Reason, int Delta)>();

        foreach (var (reason, delta) in changes.Where(change => change.Delta != 0))
        {
            var result = await mutations.ApplyAsync(
                new ReputationMutation(
                    userId,
                    role,
                    reason,
                    delta,
                    sourceService.Trim(),
                    sourceType,
                    sourceId,
                    $"{messageId:N}:{reason}",
                    "REPUTATION_V1",
                    messageId,
                    correlationId,
                    sourceId,
                    occurredAt),
                cancellationToken);

            if (result.Applied)
                applied.Add((result, reason, delta));
        }

        foreach (var item in applied)
            await PublishAsync(messageId, correlationId, occurredAt, userId, item, cancellationToken);

        if (saveChanges)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReverseAsync(
        Guid originalEntryId,
        Guid messageId,
        Guid? correlationId,
        DateTimeOffset occurredAt,
        Guid userId,
        string evidenceReference,
        CancellationToken cancellationToken)
    {
        var result = await mutations.ReverseAsync(
            originalEntryId,
            messageId,
            correlationId,
            $"{messageId:N}:reversal:{originalEntryId:N}",
            evidenceReference,
            occurredAt,
            cancellationToken);

        if (result.Applied)
            await PublishAsync(
                messageId,
                correlationId,
                occurredAt,
                userId,
                (result, "reversal", (result.ScoreAfter ?? 0) - (result.ScoreBefore ?? 0)),
                cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task PublishAsync(
        Guid messageId,
        Guid? correlationId,
        DateTimeOffset occurredAt,
        Guid userId,
        (ReputationMutationResult Result, string Reason, int Delta) item,
        CancellationToken cancellationToken)
    {
        var result = item.Result;
        await publisher.Publish(
            new ReputationScoreUpdated(
                Guid.NewGuid(),
                correlationId ?? messageId,
                occurredAt,
                userId,
                result.Role!,
                result.EntryId!.Value,
                item.Reason,
                result.ScoreBefore!.Value,
                result.ScoreAfter!.Value),
            cancellationToken);

        if (!string.Equals(
                result.TrustLevelBefore,
                result.TrustLevelAfter,
                StringComparison.Ordinal))
        {
            await publisher.Publish(
                new TrustLevelChanged(
                    Guid.NewGuid(),
                    correlationId ?? messageId,
                    occurredAt,
                    userId,
                    result.Role!,
                    result.EntryId.Value,
                    result.TrustLevelBefore!,
                    result.TrustLevelAfter!),
                cancellationToken);
        }

        if (item.Delta < 0)
        {
            await publisher.Publish(
                new ReputationPenaltyApplied(
                    Guid.NewGuid(),
                    correlationId ?? messageId,
                    occurredAt,
                    userId,
                    result.Role!,
                    result.EntryId.Value,
                    item.Reason,
                    item.Delta,
                    result.ScoreAfter.Value),
                cancellationToken);
        }
    }

    private static void Validate(
        Guid messageId,
        DateTimeOffset occurredAt,
        string sourceService,
        string sourceId,
        Guid userId)
    {
        if (messageId == Guid.Empty || userId == Guid.Empty)
            throw new ArgumentException("MessageId and UserId are required.");
        if (occurredAt == default)
            throw new ArgumentException("OccurredAt is required.");
        if (string.IsNullOrWhiteSpace(sourceService) || string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("Source metadata is required.");
    }
}
