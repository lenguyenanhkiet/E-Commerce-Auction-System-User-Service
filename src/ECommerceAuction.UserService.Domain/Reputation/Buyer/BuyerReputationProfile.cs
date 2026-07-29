using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;
/// <summary>
/// Stores the current summarized reputation state of a buyer.
///
/// This entity is a summary/read model for fast access.
/// ReputationLedgerEntry remains the auditable source of reputation changes.
/// </summary>

public sealed class BuyerReputationProfile : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    /// <summary>
    /// Points that are fully confirmed and can be used for eligibility checks.
    /// This value is allowed to become negative.
    /// </summary>
    public int ConfirmedScore { get; private set; }

    /// <summary>
    /// Points awaiting complaint-window completion or risk review.
    /// Pending points must not be used for feature eligibility.
    /// </summary>
    public int PendingScore { get; private set; }

    public long LifetimeEarnedPoints { get; private set; }
    public long LifetimePenaltyPoints { get; private set; }

    public int SuccessfulTransactions { get; private set; }
    public int FailedTransactions { get; private set; }

    public int SuccessfulAuctions { get; private set; }
    public int FailedAuctions { get; private set; }

    public int PenaltyCount { get; private set; }

    public string TrustLevel { get; private set; }
        = BuyerTrustLevels.Basic;

    private BuyerReputationProfile()
    { }

    private BuyerReputationProfile(Guid userId, DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        createdAt = createdAt.ToUniversalTime();
        Id = Guid.NewGuid();
        UserId = userId;
        ConfirmedScore = 0;
        PendingScore = 0;
        TrustLevel = BuyerTrustLevelResolver.Resolve(0);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static BuyerReputationProfile Create(Guid userId, DateTimeOffset createdAt)
    {
        return new BuyerReputationProfile(userId, createdAt);
    }

    /// <summary>
    /// Applies a confirmed reputation change.
    /// Positive points are rewards; negative points are penalties.
    /// </summary>
    ///

    public void ApplyConfirmedPoints(int points, DateTimeOffset occurredAt)
    {
        if (points == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Reputation points cannot be zero.");
        }
        occurredAt = occurredAt.ToUniversalTime();
        ConfirmedScore = checked(ConfirmedScore + points);

        if (points > 0)
        {
            LifetimeEarnedPoints = checked(LifetimeEarnedPoints + points);
        }
        else
        {
            LifetimePenaltyPoints = checked(LifetimePenaltyPoints + Math.Abs((long)points));
            PenaltyCount = checked(PenaltyCount + 1);
        }

        TrustLevel = BuyerTrustLevelResolver.Resolve(ConfirmedScore);
        UpdatedAt = occurredAt;
    }

    public void AddPendingPoints(int points, DateTimeOffset occurredAt)
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Pending reputation points must be positive.");
        }

        occurredAt = occurredAt.ToUniversalTime();
        PendingScore = checked(PendingScore + points);
        UpdatedAt = occurredAt;
    }

    public void ConfirmPendingPoints(int points, DateTimeOffset occurredAt)
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Confirmed pending points must be positive.");
        }

        if (PendingScore < points)
        {
            throw new InvalidOperationException(
                "Pending score is lower than the requested confirmation amount.");
        }

        PendingScore -= points;
        ApplyConfirmedPoints(points, occurredAt);
    }

    public void CancelPendingPoints(int points, DateTimeOffset occurredAt)
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(points),
                "Cancelled pending points must be positive.");
        }

        if (PendingScore < points)
        {
            throw new InvalidOperationException(
                "Pending score is lower than the requested cancellation amount.");
        }

        occurredAt = occurredAt.ToUniversalTime();
        PendingScore -= points;
        UpdatedAt = occurredAt;
    }

    public void RecordSuccessfulTransaction(DateTimeOffset occurredAt)
    {
        occurredAt = occurredAt.ToUniversalTime();
        SuccessfulTransactions =
            checked(SuccessfulTransactions + 1);

        UpdatedAt = occurredAt;
    }

    public void RecordFailedTransaction(DateTimeOffset occurredAt)
    {
        occurredAt = occurredAt.ToUniversalTime();
        FailedTransactions =
            checked(FailedTransactions + 1);

        UpdatedAt = occurredAt;
    }

    public void RecordSuccessfulAuction(DateTimeOffset occurredAt)
    {
        occurredAt = occurredAt.ToUniversalTime();
        SuccessfulAuctions =
            checked(SuccessfulAuctions + 1);

        UpdatedAt = occurredAt;
    }

    public void RecordFailedAuction(DateTimeOffset occurredAt)
    {
        occurredAt = occurredAt.ToUniversalTime();
        FailedAuctions =
            checked(FailedAuctions + 1);

        UpdatedAt = occurredAt;
    }
}
