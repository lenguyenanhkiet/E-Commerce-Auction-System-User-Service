namespace ECommerceAuction.UserService.Domain.Reputation.Scoring;

public static class ReputationReasonCatalog
{
    public const string BuyerProfileEmailVerified = "buyer.profile.email-verified";
    public const string BuyerProfilePhoneVerified = "buyer.profile.phone-verified";
    public const string BuyerProfileIdentityVerified = "buyer.profile.identity-verified";
    public const string BuyerProfileAddressVerified = "buyer.profile.address-verified";
    public const string BuyerProfilePaymentMethodLinked = "buyer.profile.payment-method-linked";
    public const string BuyerOrderValue = "buyer.ecommerce.order-value";
    public const string BuyerFirstOrderCompleted = "buyer.ecommerce.first-order-completed";
    public const string BuyerReviewSubmitted = "buyer.ecommerce.review-submitted";
    public const string BuyerReviewWithMedia = "buyer.ecommerce.review-with-media";
    public const string BuyerCancelledAfterPacking = "buyer.ecommerce.cancelled-after-packing";
    public const string BuyerCodRefused = "buyer.ecommerce.cod-refused";
    public const string BuyerMaliciousReturn = "buyer.ecommerce.malicious-return";
    public const string BuyerAuctionTransactionValue = "buyer.auction.transaction-value";
    public const string BuyerAuctionCommitmentBonus = "buyer.auction.commitment-bonus";
    public const string BuyerAuctionActiveParticipation = "buyer.auction.active-participation";
    public const string BuyerAuctionWinnerPaymentDefault = "buyer.auction.winner-payment-default";
    public const string SellerProfileApproved = "seller.profile.approved";
    public const string SellerOrderCompleted = "seller.ecommerce.order-completed";
    public const string SellerShipmentOnTime = "seller.ecommerce.shipment-on-time";
    public const string SellerItemAsDescribed = "seller.ecommerce.item-as-described";
    public const string SellerPositiveReview = "seller.ecommerce.positive-review";
    public const string SellerHandledWithinSla = "seller.ecommerce.handled-within-sla";
    public const string SellerHighValueOrder = "seller.ecommerce.high-value-order";
    public const string SellerAuctionTransactionValue = "seller.auction.transaction-value";
    public const string SellerAuctionCommitmentBonus = "seller.auction.commitment-bonus";
    public const string SellerAuctionFirstCompleted = "seller.auction.first-completed";
    public const string SellerAuctionShippedOnTime = "seller.auction.shipped-on-time";
    public const string SellerAuctionItemAsDescribed = "seller.auction.item-as-described";
    public const string SellerAuctionShillBiddingConfirmed = "seller.auction.shill-bidding-confirmed";

    private static readonly HashSet<string> Known =
    [
        BuyerProfileEmailVerified,
        BuyerProfilePhoneVerified,
        BuyerProfileIdentityVerified,
        BuyerProfileAddressVerified,
        BuyerProfilePaymentMethodLinked,
        BuyerOrderValue,
        BuyerFirstOrderCompleted,
        BuyerReviewSubmitted,
        BuyerReviewWithMedia,
        BuyerCancelledAfterPacking,
        BuyerCodRefused,
        BuyerMaliciousReturn,
        BuyerAuctionTransactionValue,
        BuyerAuctionCommitmentBonus,
        BuyerAuctionActiveParticipation,
        BuyerAuctionWinnerPaymentDefault,
        SellerProfileApproved,
        SellerOrderCompleted,
        SellerShipmentOnTime,
        SellerItemAsDescribed,
        SellerPositiveReview,
        SellerHandledWithinSla,
        SellerHighValueOrder,
        SellerAuctionTransactionValue,
        SellerAuctionCommitmentBonus,
        SellerAuctionFirstCompleted,
        SellerAuctionShippedOnTime,
        SellerAuctionItemAsDescribed,
        SellerAuctionShillBiddingConfirmed
    ];

    public static bool IsKnown(string? reasonCode)
    {
        return reasonCode is not null && Known.Contains(reasonCode);
    }
}
