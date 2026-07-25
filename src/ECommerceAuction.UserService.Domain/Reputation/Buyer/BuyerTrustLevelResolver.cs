using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;
/// <summary>
/// Resolves a buyer trust level from the confirmed reputation score.
/// </summary>

public static class BuyerTrustLevelResolver
{
    public static string Resolve(int confirmedScore)
    {
        return confirmedScore switch
        {
            < 0 => BuyerTrustLevels.Restricted,
            < 50 => BuyerTrustLevels.Basic,
            < 200 => BuyerTrustLevels.Trusted,
            < 500 => BuyerTrustLevels.Reliable,
            < 1_000 => BuyerTrustLevels.Premium,
            _ => BuyerTrustLevels.Elite
        };
    }
}