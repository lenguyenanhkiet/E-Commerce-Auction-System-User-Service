namespace ECommerceAuction.UserService.Domain.Reputation.Common;

public static class ReputationTrustLevelResolver
{
    public static string Resolve(int confirmedScore)
    {
        return confirmedScore switch
        {
            < 0 => ReputationTrustLevels.Restricted,
            < 50 => ReputationTrustLevels.Basic,
            < 200 => ReputationTrustLevels.Trusted,
            < 500 => ReputationTrustLevels.Reliable,
            < 1_000 => ReputationTrustLevels.Premium,
            _ => ReputationTrustLevels.Elite
        };
    }
}
