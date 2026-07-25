using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Ledger;

public static class ReputationEntryTypes
{
    public const string ProfileVerification = "PROFILE_VERIFICATION";
    public const string EcommerceTransaction = "ECOMMERCE_TRANSACTION";
    public const string AuctionTransaction = "AUCTION_TRANSACTION";
    public const string AuctionBonus = "AUCTION_BONUS";
    public const string Review = "REVIEW";
    public const string Penalty = "PENALTY";
    public const string Reversal = "REVERSAL";

    public static bool IsValid(string? value)
    {
        return value is
            ProfileVerification or
            EcommerceTransaction or
            AuctionTransaction or
            AuctionBonus or
            Review or
            Penalty or
            Reversal;
    }
}