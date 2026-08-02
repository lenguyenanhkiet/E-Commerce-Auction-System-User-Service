namespace ECommerceAuction.UserService.Domain.Reputation.Policies;

public static class CodEligibilityPolicy
{
    public static ReputationEligibilityResult Evaluate() =>
        new(
            false,
            ReputationEligibilityReasonCodes.CodPolicyNotConfigured);
}
