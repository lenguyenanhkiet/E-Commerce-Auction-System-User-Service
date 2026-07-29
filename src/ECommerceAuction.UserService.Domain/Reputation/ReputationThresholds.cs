using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation
{
    public static class ReputationThresholds
    {
        // Min score  to create a product in the ecommerce
        public const int MinScoreToCreateProduct = 21;
        // Min score to join auction for buyer
        public const int MinScoreToJoinAuction = 13;
    }
}
