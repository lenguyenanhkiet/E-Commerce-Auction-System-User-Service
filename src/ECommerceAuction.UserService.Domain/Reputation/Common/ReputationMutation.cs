namespace ECommerceAuction.UserService.Domain.Reputation.Common;

public sealed record ReputationMutation(
    Guid UserId,
    string Role,
    string ReasonCode,
    int ScoreDelta,
    string SourceService,
    string SourceType,
    string SourceId,
    string IdempotencyKey,
    string RuleVersion,
    Guid MessageId,
    Guid? CorrelationId,
    string? EvidenceReference,
    DateTimeOffset OccurredAt);
