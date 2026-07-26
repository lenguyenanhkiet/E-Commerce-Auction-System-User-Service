using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.IdentityVerifications;
/// <summary>
/// Source of truth for the five buyer profile-verification dimensions.
/// </summary>

public sealed class BuyerVerificationProfile : AuditableEntity, IAggregateRoot
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

    public bool IsFullyVerified =>
        IsEmailVerified &&
        IsPhoneVerified &&
        IsIdentityVerified &&
        HasVerifiedAddress &&
        HasVerifiedPaymentMethod;

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

    public bool VerifyEmail(DateTimeOffset occurredAt) =>
        VerifyDimension(
            IsEmailVerified,
            value => IsEmailVerified = value,
            value => EmailVerifiedAt = value,
            occurredAt);

    public bool VerifyPhone(DateTimeOffset occurredAt) =>
        VerifyDimension(
            IsPhoneVerified,
            value => IsPhoneVerified = value,
            value => PhoneVerifiedAt = value,
            occurredAt);

    public bool VerifyIdentity(DateTimeOffset occurredAt) =>
        VerifyDimension(
            IsIdentityVerified,
            value => IsIdentityVerified = value,
            value => IdentityVerifiedAt = value,
            occurredAt);

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

    private bool VerifyDimension(
        bool isVerified,
        Action<bool> setVerified,
        Action<DateTimeOffset?> setVerifiedAt,
        DateTimeOffset occurredAt)
    {
        if (isVerified)
        {
            return false;
        }

        occurredAt = occurredAt.ToUniversalTime();
        setVerified(true);
        setVerifiedAt(occurredAt);
        UpdatedAt = occurredAt;
        return true;
    }
}
