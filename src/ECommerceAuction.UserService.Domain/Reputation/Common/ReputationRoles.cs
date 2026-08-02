namespace ECommerceAuction.UserService.Domain.Reputation.Common;

public static class ReputationRoles
{
    public const string Buyer = "BUYER";
    public const string Seller = "SELLER";

    public static bool IsValid(string? role)
    {
        return role is Buyer or Seller;
    }
}
