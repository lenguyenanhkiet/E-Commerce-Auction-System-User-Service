using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

/// <summary>
/// Stable reason codes for reputation ledger entries.
/// These values should not be renamed after production release.
/// </summary>
public static class ReputationReasons
{
    public const string EmailVerified = "EMAIL_VERIFIED";
    public const string PhoneVerified = "PHONE_VERIFIED";
    public const string IdentityVerified = "IDENTITY_VERIFIED";
    public const string AddressVerified = "ADDRESS_VERIFIED";
    public const string PaymentMethodVerified = "PAYMENT_METHOD_VERIFIED";

    public const string EcommerceOrderCompleted =
        "ECOMMERCE_ORDER_COMPLETED";

    public const string FirstEcommerceOrderCompleted =
        "FIRST_ECOMMERCE_ORDER_COMPLETED";

    public const string AuctionOrderCompleted =
        "AUCTION_ORDER_COMPLETED";

    public const string AuctionCommitmentBonus =
        "AUCTION_COMMITMENT_BONUS";

    public const string AuctionParticipation =
        "AUCTION_PARTICIPATION";

    public const string ProductReviewCreated =
        "PRODUCT_REVIEW_CREATED";

    public const string ProductReviewMediaBonus =
        "PRODUCT_REVIEW_MEDIA_BONUS";

    public const string LateOrderCancellation =
        "LATE_ORDER_CANCELLATION";

    public const string CodNoShow = "COD_NO_SHOW";

    public const string MaliciousReturn =
        "MALICIOUS_RETURN";

    public const string AuctionPaymentTimeout =
        "AUCTION_PAYMENT_TIMEOUT";

    public const string PointsReversed =
        "POINTS_REVERSED";

    public static bool IsValid(string? value)
    {
        return value is
            EmailVerified or
            PhoneVerified or
            IdentityVerified or
            AddressVerified or
            PaymentMethodVerified or
            EcommerceOrderCompleted or
            FirstEcommerceOrderCompleted or
            AuctionOrderCompleted or
            AuctionCommitmentBonus or
            AuctionParticipation or
            ProductReviewCreated or
            ProductReviewMediaBonus or
            LateOrderCancellation or
            CodNoShow or
            MaliciousReturn or
            AuctionPaymentTimeout or
            PointsReversed;
    }
}