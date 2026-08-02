using ECommerceAuction.UserService.Domain.Reputation.Common;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Services;

public interface IReputationMutationService
{
    Task<ReputationMutationResult> ApplyAsync(
        ReputationMutation mutation,
        CancellationToken cancellationToken = default);

    Task<ReputationMutationResult> ReverseAsync(
        Guid originalEntryId,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default);
}

public sealed record ReputationMutationResult(
    bool Applied,
    bool Duplicate,
    Guid? EntryId,
    string? Role,
    int? ScoreBefore,
    int? ScoreAfter,
    string? TrustLevelBefore,
    string? TrustLevelAfter,
    bool RestrictionChanged)
{
    public static ReputationMutationResult AppliedResult(
        Guid entryId,
        string role,
        int scoreBefore,
        int scoreAfter,
        string trustLevelBefore,
        string trustLevelAfter,
        bool restrictionChanged = false) =>
        new(
            true,
            false,
            entryId,
            role,
            scoreBefore,
            scoreAfter,
            trustLevelBefore,
            trustLevelAfter,
            restrictionChanged);

    public static ReputationMutationResult DuplicateResult() =>
        new(false, true, null, null, null, null, null, null, false);
}
