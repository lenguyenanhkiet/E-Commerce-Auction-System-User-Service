using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers.Policies;

/// <summary>
/// To register as a seller, you must complete the following three pieces of information verification.
/// 1. Email
/// 2. Phone
/// 3. CCCD
/// </summary>
public static class SellerRegistrationRequirements
{
    public const string EmailVerification = "EMAIL_VERIFICATION";
    public const string IdentityVerification = "IDENTITY_VERIFICATION";
    public const string PhoneVerification = "PHONE_VERIFICATION";

    public static bool IsValid(string? requirement)
    {
        return requirement is EmailVerification or IdentityVerification or PhoneVerification;
    }
}