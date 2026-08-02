namespace ECommerceAuction.UserService.Domain.Reputation.Common;

public static class ReputationRestrictions
{
    public const string None = "NONE";
    public const string Restricted = "RESTRICTED";
    public const string Blocked = "BLOCKED";

    public static bool IsValid(string? status)
    {
        return status is None or Restricted or Blocked;
    }
}
