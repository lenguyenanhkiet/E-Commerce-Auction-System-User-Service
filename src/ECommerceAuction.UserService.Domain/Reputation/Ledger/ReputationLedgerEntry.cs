using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;
/// <summary>
/// Represents one immutable reputation mutation.
///
/// The entry content must not be edited after creation.
/// Lifecycle changes are limited to Pending -> Confirmed/Cancelled
/// and Confirmed -> Reversed.
/// </summary>

public sealed class ReputationLedgerEntry : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    public string EntryType { get; private set; }
        = string.Empty;

    public string Reason { get; private set; }
        = string.Empty;

    public string Status { get; private set; }
        = ReputationEntryStatuses.Pending;

    /// <summary>
    /// Positive value for rewards and negative value for penalties.
    /// Zero is not permitted.
    /// </summary>
    public int Points { get; private set; }

    /// <summary>
    /// Name of the service that originated the source event.
    /// Examples: user-service, commerce-service, auction-service.
    /// </summary>
    public string SourceService { get; private set; }
        = string.Empty;

    /// <summary>
    /// Business source type such as USER_EMAIL, ORDER or AUCTION.
    /// </summary>
    public string SourceType { get; private set; }
        = string.Empty;

    /// <summary>
    /// External or internal source identifier.
    /// </summary>
    public string SourceId { get; private set; }
        = string.Empty;

    /// <summary>
    /// Unique key used to guarantee that the same reward or penalty
    /// cannot be processed more than once.
    /// </summary>
    public string IdempotencyKey { get; private set; }
        = string.Empty;

    /// <summary>
    /// Rule version used when calculating this entry.
    /// </summary>
    public string RuleVersion { get; private set; }
        = "REPUTATION_V1";

    public Guid? ReversalEntryId { get; private set; }

    public DateTime? ConfirmAfter { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? ReversedAt { get; private set; }

    protected ReputationLedgerEntry()
    {
    }

    private ReputationLedgerEntry(
        Guid userId,
        string entryType,
        string reason,
        string status,
        int points,
        string sourceService,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        string ruleVersion,
        DateTime createdAt,
        DateTime? confirmAfter)
    {
        Validate(
            userId,
            entryType,
            reason,
            status,
            points,
            sourceService,
            sourceType,
            sourceId,
            idempotencyKey,
            ruleVersion);

        Id = Guid.NewGuid();
        UserId = userId;
        EntryType = entryType;
        Reason = reason;
        Status = status;
        Points = points;
        SourceService = sourceService.Trim();
        SourceType = sourceType.Trim();
        SourceId = sourceId.Trim();
        IdempotencyKey = idempotencyKey.Trim();
        RuleVersion = ruleVersion.Trim();
        ConfirmAfter = confirmAfter;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;

        if (status == ReputationEntryStatuses.Confirmed)
        {
            ConfirmedAt = createdAt;
        }
    }

    public static ReputationLedgerEntry CreateConfirmed(
        Guid userId,
        string entryType,
        string reason,
        int points,
        string sourceService,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        DateTime occurredAt,
        string ruleVersion = "REPUTATION_V1")
    {
        return new ReputationLedgerEntry(
            userId,
            entryType,
            reason,
            ReputationEntryStatuses.Confirmed,
            points,
            sourceService,
            sourceType,
            sourceId,
            idempotencyKey,
            ruleVersion,
            occurredAt,
            confirmAfter: null);
    }

    public static ReputationLedgerEntry CreatePending(
        Guid userId,
        string entryType,
        string reason,
        int points,
        string sourceService,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        DateTime occurredAt,
        DateTime confirmAfter,
        string ruleVersion = "REPUTATION_V1")
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Pending reputation entries must have positive points.");
        }

        if (confirmAfter <= occurredAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confirmAfter),
                "Confirmation time must be after occurrence time.");
        }

        return new ReputationLedgerEntry(
            userId,
            entryType,
            reason,
            ReputationEntryStatuses.Pending,
            points,
            sourceService,
            sourceType,
            sourceId,
            idempotencyKey,
            ruleVersion,
            occurredAt,
            confirmAfter);
    }

    public void Confirm(DateTime occurredAt)
    {
        if (Status != ReputationEntryStatuses.Pending)
        {
            throw new InvalidOperationException(
                "Only pending reputation entries can be confirmed.");
        }

        if (ConfirmAfter.HasValue &&
            occurredAt < ConfirmAfter.Value)
        {
            throw new InvalidOperationException(
                "The reputation entry cannot be confirmed before ConfirmAfter.");
        }

        Status = ReputationEntryStatuses.Confirmed;
        ConfirmedAt = occurredAt;
        UpdatedAt = occurredAt;
    }

    public void Cancel(DateTime occurredAt)
    {
        if (Status != ReputationEntryStatuses.Pending)
        {
            throw new InvalidOperationException(
                "Only pending reputation entries can be cancelled.");
        }

        Status = ReputationEntryStatuses.Cancelled;
        CancelledAt = occurredAt;
        UpdatedAt = occurredAt;
    }

    public void MarkReversed(
        Guid reversalEntryId,
        DateTime occurredAt)
    {
        if (Status != ReputationEntryStatuses.Confirmed)
        {
            throw new InvalidOperationException(
                "Only confirmed reputation entries can be reversed.");
        }

        if (reversalEntryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reversal entry ID cannot be empty.",
                nameof(reversalEntryId));
        }

        Status = ReputationEntryStatuses.Reversed;
        ReversalEntryId = reversalEntryId;
        ReversedAt = occurredAt;
        UpdatedAt = occurredAt;
    }

    private static void Validate(
        Guid userId,
        string entryType,
        string reason,
        string status,
        int points,
        string sourceService,
        string sourceType,
        string sourceId,
        string idempotencyKey,
        string ruleVersion)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (!ReputationEntryTypes.IsValid(entryType))
        {
            throw new ArgumentException(
                $"Invalid reputation entry type: {entryType}.",
                nameof(entryType));
        }

        if (!ReputationReasons.IsValid(reason))
        {
            throw new ArgumentException(
                $"Invalid reputation reason: {reason}.",
                nameof(reason));
        }

        if (!ReputationEntryStatuses.IsValid(status))
        {
            throw new ArgumentException(
                $"Invalid reputation entry status: {status}.",
                nameof(status));
        }

        if (points == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Reputation points cannot be zero.");
        }

        if (string.IsNullOrWhiteSpace(sourceService))
        {
            throw new ArgumentException(
                "Source service is required.",
                nameof(sourceService));
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            throw new ArgumentException(
                "Source type is required.",
                nameof(sourceType));
        }

        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new ArgumentException(
                "Source ID is required.",
                nameof(sourceId));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException(
                "Idempotency key is required.",
                nameof(idempotencyKey));
        }

        if (string.IsNullOrWhiteSpace(ruleVersion))
        {
            throw new ArgumentException(
                "Rule version is required.",
                nameof(ruleVersion));
        }
    }
}