using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Reputation
{
    public static class ReputationPoints
    {
        // Reputation points for user join auction platform
        public const int EmailVerifiedPoint = 1;
        public const int PhoneVerifiedPoint = 2;
        public const int VerifiedPaymentMethodPoint = 3;
        public const int IdentificationVerifiedPoint = 5;
        // Reputation points for user become a seller
        public const int TaxVerifiedPoint = 2;
        public const int BusinessLicenseVerifiedPoint = 5;
        public const int BusinessAddressVerifiedPoint = 3;
    }
}
