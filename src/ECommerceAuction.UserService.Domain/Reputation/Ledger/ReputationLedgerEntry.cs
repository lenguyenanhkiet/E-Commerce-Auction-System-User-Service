using ECommerceAuction.UserService.Domain.Common;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

public sealed class ReputationLedgerEntry : AuditableEntity, IAggregateRoot
{
    private ReputationLedgerEntry()
    {
    }

    public Guid UserId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string ReasonCode { get; private set; } = string.Empty;
    public int ScoreDelta { get; private set; }
    public int ScoreBefore { get; private set; }
    public int ScoreAfter { get; private set; }
    public string SourceService { get; private set; } = string.Empty;
    public string SourceType { get; private set; } = string.Empty;
    public string SourceId { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RuleVersion { get; private set; } = string.Empty;
    public Guid MessageId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? ReversesEntryId { get; private set; }
    public string? EvidenceReference { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    public static ReputationLedgerEntry Create(
        ReputationMutation mutation,
        int scoreBefore,
        int scoreAfter,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        Validate(mutation);

        int calculated;
        try
        {
            calculated = checked(scoreBefore + mutation.ScoreDelta);
        }
        catch (OverflowException)
        {
            throw;
        }

        if (calculated != scoreAfter)
        {
            throw new InvalidOperationException(
                "ScoreAfter must equal ScoreBefore plus ScoreDelta.");
        }

        var occurredAt = mutation.OccurredAt.ToUniversalTime();
        createdAt = createdAt.ToUniversalTime();

        return new ReputationLedgerEntry
        {
            Id = Guid.NewGuid(),
            UserId = mutation.UserId,
            Role = mutation.Role,
            ReasonCode = mutation.ReasonCode,
            ScoreDelta = mutation.ScoreDelta,
            ScoreBefore = scoreBefore,
            ScoreAfter = scoreAfter,
            SourceService = mutation.SourceService.Trim(),
            SourceType = mutation.SourceType.Trim(),
            SourceId = mutation.SourceId.Trim(),
            IdempotencyKey = mutation.IdempotencyKey.Trim(),
            RuleVersion = mutation.RuleVersion.Trim(),
            MessageId = mutation.MessageId,
            CorrelationId = mutation.CorrelationId,
            EvidenceReference = Normalize(mutation.EvidenceReference),
            OccurredAt = occurredAt,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public static ReputationLedgerEntry CreateReversal(
        ReputationLedgerEntry original,
        int scoreBefore,
        Guid messageId,
        Guid? correlationId,
        string idempotencyKey,
        string evidenceReference,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(original);
        if (original.ReversesEntryId.HasValue)
        {
            throw new InvalidOperationException(
                "A reversal entry cannot be reversed with this factory.");
        }

        var reversalDelta = checked(-original.ScoreDelta);
        var scoreAfter = checked(scoreBefore + reversalDelta);
        var mutation = new ReputationMutation(
            original.UserId,
            original.Role,
            original.ReasonCode,
            reversalDelta,
            original.SourceService,
            original.SourceType,
            original.SourceId,
            idempotencyKey,
            original.RuleVersion,
            messageId,
            correlationId,
            evidenceReference,
            occurredAt);

        var reversal = Create(mutation, scoreBefore, scoreAfter, createdAt);
        reversal.ReversesEntryId = original.Id;
        return reversal;
    }

    // Compatibility factory for existing verification flows. New code uses Create.
    public static ReputationLedgerEntry CreateConfirmed(
        Guid userId,
        string entryType,
        string reason,
        int points,
        string sourceService,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        DateTimeOffset occurredAt,
        string ruleVersion = "REPUTATION_V1")
    {
        var reasonCode = MapLegacyReason(reason);
        var mutation = new ReputationMutation(
            userId,
            ReputationRoles.Buyer,
            reasonCode,
            points,
            sourceService,
            sourceType,
            sourceId,
            idempotencyKey,
            ruleVersion,
            CreateDeterministicCompatibilityMessageId(idempotencyKey),
            null,
            null,
            occurredAt);

        return Create(mutation, 0, points, occurredAt);
    }

    private static void Validate(ReputationMutation mutation)
    {
        if (mutation.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(mutation));
        if (!ReputationRoles.IsValid(mutation.Role))
            throw new ArgumentException("Invalid reputation role.", nameof(mutation));
        if (!ReputationReasonCatalog.IsKnown(mutation.ReasonCode))
            throw new ArgumentException("Invalid reputation reason code.", nameof(mutation));
        if (mutation.ScoreDelta == 0)
            throw new ArgumentOutOfRangeException(nameof(mutation), "Score delta cannot be zero.");
        if (mutation.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(mutation));
        Require(mutation.SourceService, "Source service");
        Require(mutation.SourceType, "Source type");
        Require(mutation.SourceId, "Source ID");
        Require(mutation.IdempotencyKey, "Idempotency key");
        Require(mutation.RuleVersion, "Rule version");
    }

    private static void Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} is required.");
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid CreateDeterministicCompatibilityMessageId(string key)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(key));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static string MapLegacyReason(string reason) => reason switch
    {
        ReputationReasons.EmailVerified => ReputationReasonCatalog.BuyerProfileEmailVerified,
        ReputationReasons.PhoneVerified => ReputationReasonCatalog.BuyerProfilePhoneVerified,
        ReputationReasons.IdentityVerified => ReputationReasonCatalog.BuyerProfileIdentityVerified,
        ReputationReasons.AddressVerified => ReputationReasonCatalog.BuyerProfileAddressVerified,
        ReputationReasons.PaymentMethodVerified => ReputationReasonCatalog.BuyerProfilePaymentMethodLinked,
        _ => throw new ArgumentException("Unsupported legacy reputation reason.", nameof(reason))
    };
}
