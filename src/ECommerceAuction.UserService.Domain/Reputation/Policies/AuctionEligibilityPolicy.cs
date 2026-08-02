using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Seller;

namespace ECommerceAuction.UserService.Domain.Reputation.Policies;

public sealed record ReputationEligibilityResult(
    bool IsEligible,
    string ReasonCode);

public static class ReputationEligibilityReasonCodes
{
    public const string Eligible = "ELIGIBLE";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string SellerInactive = "SELLER_INACTIVE";
    public const string ScoreTooLow = "SCORE_TOO_LOW";
    public const string PriorDefaultScoreTooLow = "PRIOR_DEFAULT_SCORE_TOO_LOW";
    public const string BlockingViolation = "BLOCKING_VIOLATION";
    public const string ManualReviewRequired = "MANUAL_REVIEW_REQUIRED";
    public const string AuctionRestricted = "AUCTION_RESTRICTED";
    public const string SellingRestricted = "SELLING_RESTRICTED";
    public const string CodPolicyNotConfigured = "COD_POLICY_NOT_CONFIGURED";
}

public static class AuctionEligibilityPolicy
{
    public static ReputationEligibilityResult EvaluateBuyer(
        BuyerReputationProfile profile,
        bool isActive,
        bool hasPriorPaymentDefault,
        DateTimeOffset evaluatedAt)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!isActive)
            return Denied(ReputationEligibilityReasonCodes.AccountInactive);
        if (!string.IsNullOrWhiteSpace(profile.BlockingViolationCode))
            return Denied(ReputationEligibilityReasonCodes.BlockingViolation);
        if (profile.RequiresManualReview)
            return Denied(ReputationEligibilityReasonCodes.ManualReviewRequired);

        if (hasPriorPaymentDefault)
        {
            if (profile.AuctionRestrictionStatus != ReputationRestrictions.None)
                return Denied(ReputationEligibilityReasonCodes.AuctionRestricted);
            if (profile.ConfirmedScore <= 11)
                return Denied(
                    ReputationEligibilityReasonCodes.PriorDefaultScoreTooLow);
        }
        else
        {
            if (IsRestrictionActive(
                    profile.AuctionRestrictionStatus,
                    profile.RestrictedUntil,
                    evaluatedAt))
            {
                return Denied(ReputationEligibilityReasonCodes.AuctionRestricted);
            }

            if (profile.ConfirmedScore < 11)
                return Denied(ReputationEligibilityReasonCodes.ScoreTooLow);
        }

        return Eligible();
    }

    public static ReputationEligibilityResult EvaluateSeller(
        SellerReputationProfile profile,
        bool isActive,
        DateTimeOffset evaluatedAt)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!isActive)
            return Denied(ReputationEligibilityReasonCodes.SellerInactive);
        if (!string.IsNullOrWhiteSpace(profile.BlockingViolationCode))
            return Denied(ReputationEligibilityReasonCodes.BlockingViolation);
        if (profile.RequiresManualReview)
            return Denied(ReputationEligibilityReasonCodes.ManualReviewRequired);
        if (IsRestrictionActive(
                profile.SellingRestrictionStatus,
                profile.RestrictedUntil,
                evaluatedAt))
        {
            return Denied(ReputationEligibilityReasonCodes.SellingRestricted);
        }

        if (IsRestrictionActive(
                profile.AuctionRestrictionStatus,
                profile.RestrictedUntil,
                evaluatedAt))
        {
            return Denied(ReputationEligibilityReasonCodes.AuctionRestricted);
        }

        return profile.ConfirmedScore >= 20
            ? Eligible()
            : Denied(ReputationEligibilityReasonCodes.ScoreTooLow);
    }

    private static bool IsRestrictionActive(
        string status,
        DateTimeOffset? restrictedUntil,
        DateTimeOffset evaluatedAt) =>
        status == ReputationRestrictions.Blocked ||
        (status != ReputationRestrictions.None &&
         (!restrictedUntil.HasValue ||
          restrictedUntil.Value > evaluatedAt.ToUniversalTime()));

    private static ReputationEligibilityResult Eligible() =>
        new(true, ReputationEligibilityReasonCodes.Eligible);

    private static ReputationEligibilityResult Denied(string reasonCode) =>
        new(false, reasonCode);
}
