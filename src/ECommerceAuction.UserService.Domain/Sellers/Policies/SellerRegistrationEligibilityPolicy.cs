using ECommerceAuction.UserService.Domain.IdentityVerifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers.Policies;

public static class SellerRegistrationEligibilityPolicy
{
    public static SellerRegistrationEligibilityResult Evaluate(BuyerVerificationProfile verificationProfile)
    {
        ArgumentNullException.ThrowIfNull(verificationProfile);

        var missingRequirements = new List<string>();

        if (!verificationProfile.IsEmailVerified)
        {
            missingRequirements.Add(SellerRegistrationRequirements.EmailVerification);
        }
        if (!verificationProfile.IsPhoneVerified)
        {
            missingRequirements.Add(SellerRegistrationRequirements.PhoneVerification);
        }
        if (!verificationProfile.IsIdentityVerified)
        {
            missingRequirements.Add(SellerRegistrationRequirements.IdentityVerification);
        }
        return missingRequirements.Count == 0 ? SellerRegistrationEligibilityResult.Eligible() : SellerRegistrationEligibilityResult.NotEligible(
                missingRequirements);
    }
}