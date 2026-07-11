using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Users
{
    /// <summary>
    /// Defines user account status constants used throughout the system.
    ///User account states are used throughout the system.
    /// </summary>
    public static class UserStatus
    {
        /// <summary>
        ///The account is working normally, users can log in and use all features.
        /// Verified and functioning normally.
        /// </summary>
        public const string Active = "ACTIVE";

        /// <summary>
        ///The account is disabled due to user self-lock or request.
        /// Users lock their own accounts.
        /// </summary>
        public const string Inactive = "INACTIVE";

        /// <summary>
        ///Accounts are permanently banned for violating rules (usually enforced by Admin).
        /// Permanent ban (as punishment by Admin).
        /// </summary>
        public const string Banned = "BANNED";

        /// <summary>
        ///The account is temporarily locked due to many consecutive incorrect password login attempts.
        /// Temporarily locked due to too many incorrect password attempts.
        /// </summary>
        public const string Locked = "LOCKED";

        /// <summary>
        /// Minor/moderate violations: spam, repeated reports, minor auction fraud.
        /// Time period / Interval: Time-limited (a few days to a few weeks); expires automatically.
        /// </summary>
        public const string Blocked = "BLOCKED";

        /// <summary>
        ///Account has limited features - can log in but not allowed to bid.
        /// Feature restrictions: Login is possible but bidding is prohibited.
        /// </summary>
        public const string Restricted = "RESTRICTED";
    }
}
