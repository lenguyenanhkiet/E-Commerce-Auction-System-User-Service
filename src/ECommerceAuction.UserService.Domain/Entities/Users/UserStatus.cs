using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Users
{
    public static class UserStatus
    {
        // Verified and functioning normally.
        public const string Active = "ACTIVE";
        // Users lock their own accounts.
        public const string Inactive = "INACTIVE";
        // Permanent ban (as punishment by Admin)
        public const string Banned = "BANNED";
        // Temporarily locked (due to too many incorrect password attempts
        public const string Locked = "LOCKED";
        //Feature restrictions(Login is possible but bidding is prohibited)
        public const string Restricted = "RESTRICTED";
    }
}
