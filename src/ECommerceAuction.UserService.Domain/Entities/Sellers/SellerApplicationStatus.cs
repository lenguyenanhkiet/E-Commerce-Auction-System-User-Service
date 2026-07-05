using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Sellers
{
    public static class SellerApplicationStatus
    {
        public const string Pending = "Pending";
        public const string UnderReview = "UnderReview";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
