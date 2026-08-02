using ECommerceAuction.UserService.Domain.Common;
using ECommerceAuction.UserService.Domain.Reputation.Common;

namespace ECommerceAuction.UserService.Domain.Reputation.Seller;

public sealed class SellerReputationProfile : AuditableEntity, IAggregateRoot
{
    private SellerReputationProfile()
    {
    }

    public Guid UserId { get; private set; }
    public int ConfirmedScore { get; private set; }
    public string SellingRestrictionStatus { get; private set; }
        = ReputationRestrictions.None;
    public string AuctionRestrictionStatus { get; private set; }
        = ReputationRestrictions.None;
    public DateTimeOffset? RestrictedUntil { get; private set; }
    public bool RequiresManualReview { get; private set; }
    public string? BlockingViolationCode { get; private set; }
    public string TrustLevel =>
        ReputationTrustLevelResolver.Resolve(ConfirmedScore);

    public static SellerReputationProfile Create(
        Guid userId,
        DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        createdAt = createdAt.ToUniversalTime();
        return new SellerReputationProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void ApplyConfirmedDelta(int delta, DateTimeOffset occurredAt)
    {
        if (delta == 0)
            throw new ArgumentOutOfRangeException(nameof(delta));

        ConfirmedScore = checked(ConfirmedScore + delta);
        UpdatedAt = occurredAt.ToUniversalTime();
    }

    public void ApplySellingRestriction(
        string status,
        DateTimeOffset? restrictedUntil,
        bool requiresManualReview,
        string? blockingViolationCode,
        DateTimeOffset occurredAt) =>
        ApplyRestriction(
            status, restrictedUntil, requiresManualReview,
            blockingViolationCode, occurredAt, selling: true);

    public void ApplyAuctionRestriction(
        string status,
        DateTimeOffset? restrictedUntil,
        bool requiresManualReview,
        string? blockingViolationCode,
        DateTimeOffset occurredAt) =>
        ApplyRestriction(
            status, restrictedUntil, requiresManualReview,
            blockingViolationCode, occurredAt, selling: false);

    public void ClearRestrictions(DateTimeOffset occurredAt)
    {
        SellingRestrictionStatus = ReputationRestrictions.None;
        AuctionRestrictionStatus = ReputationRestrictions.None;
        RestrictedUntil = null;
        RequiresManualReview = false;
        BlockingViolationCode = null;
        UpdatedAt = occurredAt.ToUniversalTime();
    }

    private void ApplyRestriction(
        string status,
        DateTimeOffset? restrictedUntil,
        bool requiresManualReview,
        string? blockingViolationCode,
        DateTimeOffset occurredAt,
        bool selling)
    {
        if (!ReputationRestrictions.IsValid(status) ||
            status == ReputationRestrictions.None)
        {
            throw new ArgumentException("Invalid restriction status.", nameof(status));
        }

        if (selling)
            SellingRestrictionStatus = status;
        else
            AuctionRestrictionStatus = status;

        RestrictedUntil = restrictedUntil?.ToUniversalTime();
        RequiresManualReview = requiresManualReview;
        BlockingViolationCode = string.IsNullOrWhiteSpace(blockingViolationCode)
            ? null
            : blockingViolationCode.Trim();
        UpdatedAt = occurredAt.ToUniversalTime();
    }
}
