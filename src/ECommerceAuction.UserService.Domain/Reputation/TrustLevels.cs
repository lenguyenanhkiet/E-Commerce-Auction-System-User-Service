using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation
{
    /// <summary>
    /// Defines the trust levels for users in the system. These levels can be used to categorize users based on their reputation, activity, or other criteria defined by the business logic.
    /// </summary>
    public static class TrustLevel
    {
        public const string Silver = "SILVER";
        public const string Gold = "GOLD";
        public const string Platinum = "PLATINUM";
        public const string Diamond = "DIAMOND";
    }
}
