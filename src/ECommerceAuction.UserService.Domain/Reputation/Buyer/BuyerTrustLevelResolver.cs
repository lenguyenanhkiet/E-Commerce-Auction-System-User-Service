using ECommerceAuction.UserService.Domain.Reputation.Common;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;
/// <summary>
/// Resolves a buyer trust level from the confirmed reputation score.
/// </summary>

public static class BuyerTrustLevelResolver
{
    public static string Resolve(int confirmedScore)
    {
        return ReputationTrustLevelResolver.Resolve(confirmedScore);
    }
}
