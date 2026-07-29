using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation
{
    /// <summary>
    /// Conversion rate between transaction amount (VND) and reputation points.
    /// This is a RATE, not a fixed reward — points scale linearly with amount.
    /// Business rule: every 10,000 VND of successful transaction/auction value = 1 point.
    /// </summary>
    public static class ReputationConversionRates
    {
        public const decimal VndPerPoint = 10_000m; 
    }
}
