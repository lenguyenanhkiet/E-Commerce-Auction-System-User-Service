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
    public const string BuyerAdminAdjustment = "buyer.admin.adjustment";
    public const string SellerProfileApproved = "seller.profile.approved";
    public const string SellerOrderCompleted = "seller.ecommerce.order-completed";
    public const string SellerShipmentOnTime = "seller.ecommerce.shipment-on-time";
    public const string SellerItemAsDescribed = "seller.ecommerce.item-as-described";
    public const string SellerPositiveReview = "seller.ecommerce.positive-review";
    public const string SellerHandledWithinSla = "seller.ecommerce.handled-within-sla";
    public const string SellerHighValueOrder = "seller.ecommerce.high-value-order";
    public const string SellerShipmentLate = "seller.ecommerce.shipment-late";
    public const string SellerSevereViolation = "seller.ecommerce.severe-violation";
    public const string SellerOutOfStock = "seller.ecommerce.out-of-stock";
    public const string SellerWrongPrice = "seller.ecommerce.wrong-price";
    public const string SellerNotFulfilled = "seller.ecommerce.not-fulfilled";
    public const string SellerNotAsDescribed = "seller.ecommerce.not-as-described";
    public const string SellerImportantInformationMissing = "seller.ecommerce.important-information-missing";
    public const string SellerFakeTracking = "seller.ecommerce.fake-tracking";
    public const string SellerFaultDispute = "seller.ecommerce.seller-fault-dispute";
    public const string SellerCounterfeit = "seller.ecommerce.counterfeit";
    public const string SellerFraud = "seller.ecommerce.fraud";
    public const string SellerAdminCancelled = "seller.ecommerce.admin-cancelled";
    public const string SellerAuctionTransactionValue = "seller.auction.transaction-value";
    public const string SellerAuctionCommitmentBonus = "seller.auction.commitment-bonus";
    public const string SellerAuctionFirstCompleted = "seller.auction.first-completed";
    public const string SellerAuctionShippedOnTime = "seller.auction.shipped-on-time";
    public const string SellerAuctionItemAsDescribed = "seller.auction.item-as-described";
    public const string SellerAuctionShillBiddingConfirmed = "seller.auction.shill-bidding-confirmed";
    public const string SellerAuctionCancelledAfterBid = "seller.auction.cancelled-after-bid";
    public const string SellerAuctionCancelledNearEnd = "seller.auction.cancelled-near-end";
    public const string SellerAuctionRefusedWinningPrice = "seller.auction.refused-winning-price";
    public const string SellerAuctionNotFulfilled = "seller.auction.not-fulfilled";
    public const string SellerAuctionShipmentLateMinor = "seller.auction.shipment-late-minor";
    public const string SellerAuctionShipmentLateMajor = "seller.auction.shipment-late-major";
    public const string SellerAuctionShipmentLateSevere = "seller.auction.shipment-late-severe";
    public const string SellerAuctionMismatchMinor = "seller.auction.mismatch-minor";
    public const string SellerAuctionMismatchMajor = "seller.auction.mismatch-major";
    public const string SellerAuctionMismatchSevere = "seller.auction.mismatch-severe";
    public const string SellerAuctionCounterfeit = "seller.auction.counterfeit";
    public const string SellerAuctionFakeTracking = "seller.auction.fake-tracking";
    public const string SellerAuctionFakeTrackingSevere = "seller.auction.fake-tracking-severe";
    public const string SellerAdminAdjustment = "seller.admin.adjustment";

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
        BuyerAdminAdjustment,
        SellerProfileApproved,
        SellerOrderCompleted,
        SellerShipmentOnTime,
        SellerItemAsDescribed,
        SellerPositiveReview,
        SellerHandledWithinSla,
        SellerHighValueOrder,
        SellerShipmentLate,
        SellerSevereViolation,
        SellerOutOfStock,
        SellerWrongPrice,
        SellerNotFulfilled,
        SellerNotAsDescribed,
        SellerImportantInformationMissing,
        SellerFakeTracking,
        SellerFaultDispute,
        SellerCounterfeit,
        SellerFraud,
        SellerAdminCancelled,
        SellerAuctionTransactionValue,
        SellerAuctionCommitmentBonus,
        SellerAuctionFirstCompleted,
        SellerAuctionShippedOnTime,
        SellerAuctionItemAsDescribed,
        SellerAuctionShillBiddingConfirmed,
        SellerAuctionCancelledAfterBid,
        SellerAuctionCancelledNearEnd,
        SellerAuctionRefusedWinningPrice,
        SellerAuctionNotFulfilled,
        SellerAuctionShipmentLateMinor,
        SellerAuctionShipmentLateMajor,
        SellerAuctionShipmentLateSevere,
        SellerAuctionMismatchMinor,
        SellerAuctionMismatchMajor,
        SellerAuctionMismatchSevere,
        SellerAuctionCounterfeit,
        SellerAuctionFakeTracking,
        SellerAuctionFakeTrackingSevere,
        SellerAdminAdjustment
    ];

    public static bool IsKnown(string? reasonCode)
    {
        return reasonCode is not null && Known.Contains(reasonCode);
    }
}
