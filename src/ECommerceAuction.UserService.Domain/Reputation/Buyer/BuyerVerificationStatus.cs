using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;

public sealed class BuyerVerificationStatus
{
    public Guid UserId { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsIdentityVerified { get; private set; }
    public bool HasVerifiedAddress { get; private set; }
    public bool HasVerifiedPaymentMethod { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }
    public DateTimeOffset? PhoneVerifiedAt { get; private set; }
    public DateTimeOffset? IdentityVerifiedAt { get; private set; }
    public DateTimeOffset? AddressVerifiedAt { get; private set; }
    public DateTimeOffset? PaymentMethodVerifiedAt { get; private set; }

    public bool IsFullyVerified => IsEmailVerified && IsPhoneVerified && IsIdentityVerified && HasVerifiedAddress && HasVerifiedPaymentMethod;

    private BuyerVerificationStatus()
    {
    }

    private BuyerVerificationStatus(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }
        UserId = userId;
    }

    public static BuyerVerificationStatus Create(Guid userId)
        => new(userId);

    public bool VerifyEmail(DateTimeOffset verifiedAt)
    {
        if (IsEmailVerified)
        {
            return false;
        }

        IsEmailVerified = true;
        EmailVerifiedAt = verifiedAt;

        return true;
    }
}