using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.IdentityVerifications;
/// <summary>
/// Stores the summarized verification state used for buyer eligibility.
///
/// Email, phone and identity verification may be completed through
/// independent workflows.
/// </summary>

public sealed class BuyerVerificationProfile : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsIdentityVerified { get; private set; }

    public bool HasVerifiedAddress { get; private set; }
    public bool HasVerifiedPaymentMethod { get; private set; }

    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? PhoneVerifiedAt { get; private set; }
    public DateTime? IdentityVerifiedAt { get; private set; }
    public DateTime? AddressVerifiedAt { get; private set; }
    public DateTime? PaymentMethodVerifiedAt { get; private set; }

    public bool IsFullyVerified =>
        IsEmailVerified &&
        IsPhoneVerified &&
        IsIdentityVerified &&
        HasVerifiedAddress &&
        HasVerifiedPaymentMethod;

    protected BuyerVerificationProfile()
    {
    }

    private BuyerVerificationProfile(
        Guid userId,
        DateTime createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static BuyerVerificationProfile Create(
        Guid userId,
        DateTime createdAt)
    {
        return new BuyerVerificationProfile(userId, createdAt);
    }

    /// <returns>True when this is the first successful verification.</returns>
    public bool VerifyEmail(DateTime occurredAt)
    {
        if (IsEmailVerified)
        {
            return false;
        }

        IsEmailVerified = true;
        EmailVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool VerifyPhone(DateTime occurredAt)
    {
        if (IsPhoneVerified)
        {
            return false;
        }

        IsPhoneVerified = true;
        PhoneVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool VerifyIdentity(DateTime occurredAt)
    {
        if (IsIdentityVerified)
        {
            return false;
        }

        IsIdentityVerified = true;
        IdentityVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool VerifyAddress(DateTime occurredAt)
    {
        if (HasVerifiedAddress)
        {
            return false;
        }

        HasVerifiedAddress = true;
        AddressVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool VerifyPaymentMethod(DateTime occurredAt)
    {
        if (HasVerifiedPaymentMethod)
        {
            return false;
        }

        HasVerifiedPaymentMethod = true;
        PaymentMethodVerifiedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool RevokePhoneVerification(DateTime occurredAt)
    {
        if (!IsPhoneVerified)
        {
            return false;
        }

        IsPhoneVerified = false;
        PhoneVerifiedAt = null;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool RevokeIdentityVerification(DateTime occurredAt)
    {
        if (!IsIdentityVerified)
        {
            return false;
        }

        IsIdentityVerified = false;
        IdentityVerifiedAt = null;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool RevokePaymentMethodVerification(DateTime occurredAt)
    {
        if (!HasVerifiedPaymentMethod)
        {
            return false;
        }

        HasVerifiedPaymentMethod = false;
        PaymentMethodVerifiedAt = null;
        UpdatedAt = occurredAt;

        return true;
    }
}