using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;
/// <summary>
/// Fixed reputation rewards for one-time buyer profile verification actions.
/// Each reward may only be granted once per real user identity.
/// </summary>

public static class BuyerReputationPoints
{
    public const int EmailVerified = 1;
    public const int PhoneVerified = 2;
    public const int IdentityVerified = 5;
    public const int AddressVerified = 1;
    public const int PaymentMethodVerified = 2;

    public const int MaximumProfileVerificationScore =
        EmailVerified +
        PhoneVerified +
        IdentityVerified +
        AddressVerified +
        PaymentMethodVerified;
}