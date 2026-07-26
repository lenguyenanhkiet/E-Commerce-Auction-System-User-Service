using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.IdentityVerifications;
/// <summary>
/// Stores the summarized verification state used for buyer eligibility.
///
/// Address and payment verification may be completed through independent
/// buyer-eligibility workflows. Account email, phone and identity verification
/// belong to User and are deliberately not duplicated here.
/// </summary>

public sealed class BuyerVerificationProfile : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    public bool HasVerifiedAddress { get; private set; }
    public bool HasVerifiedPaymentMethod { get; private set; }

    public DateTimeOffset? AddressVerifiedAt { get; private set; }
    public DateTimeOffset? PaymentMethodVerifiedAt { get; private set; }

    private BuyerVerificationProfile()
    {
    }

    private BuyerVerificationProfile(
        Guid userId,
        DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        createdAt = createdAt.ToUniversalTime();
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static BuyerVerificationProfile Create(
        Guid userId,
        DateTimeOffset createdAt)
    {
        return new BuyerVerificationProfile(userId, createdAt);
    }

    public bool VerifyAddress(DateTimeOffset occurredAt)
    {
        if (HasVerifiedAddress)
        {
            return false;
        }

        occurredAt = occurredAt.ToUniversalTime();
        HasVerifiedAddress = true;
        AddressVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool VerifyPaymentMethod(DateTimeOffset occurredAt)
    {
        if (HasVerifiedPaymentMethod)
        {
            return false;
        }

        occurredAt = occurredAt.ToUniversalTime();
        HasVerifiedPaymentMethod = true;
        PaymentMethodVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool RevokePaymentMethodVerification(DateTimeOffset occurredAt)
    {
        if (!HasVerifiedPaymentMethod)
        {
            return false;
        }

        occurredAt = occurredAt.ToUniversalTime();
        HasVerifiedPaymentMethod = false;
        PaymentMethodVerifiedAt = null;
        UpdatedAt = occurredAt;

        return true;
    }
}
