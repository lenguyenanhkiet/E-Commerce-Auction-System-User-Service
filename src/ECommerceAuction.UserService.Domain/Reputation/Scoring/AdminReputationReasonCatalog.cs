namespace ECommerceAuction.UserService.Domain.Reputation.Scoring;

public static class AdminReputationReasonCatalog
{
    public static bool IsAllowed(string? reason) =>
        reason is "ADMIN_CORRECTION" or "APPEAL_APPROVED" or "FRAUD_CONFIRMED";
}
