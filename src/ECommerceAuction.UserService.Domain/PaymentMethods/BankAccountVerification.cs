using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.PaymentMethods;

/// <summary>
/// Represents the verification lifecycle of one bank account.
///
/// The full account number must not be stored in this entity.
/// Store only a masked account number and a secure fingerprint.
/// </summary>
public sealed class BankAccountVerification
    : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    public string Provider { get; private set; } = string.Empty;

    public string ProviderReference { get; private set; } = string.Empty;

    public string BankCode { get; private set; } = string.Empty;

    public string MaskedAccountNumber { get; private set; } = string.Empty;

    public string AccountFingerprint { get; private set; } = string.Empty;

    public string ExpectedAccountHolderName { get; private set; }
        = string.Empty;

    public string? VerifiedAccountHolderName { get; private set; }

    public string Status { get; private set; }
        = BankAccountVerificationStatuses.Pending;

    public string? FailureCode { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset? VerifiedAt { get; private set; }

    public DateTimeOffset? RejectedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    private BankAccountVerification()
    {
    }

    private BankAccountVerification(
        Guid userId,
        string provider,
        string providerReference,
        string bankCode,
        string maskedAccountNumber,
        string accountFingerprint,
        string expectedAccountHolderName,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (!BankVerificationProviders.IsValid(provider))
        {
            throw new ArgumentException(
                $"Unsupported bank verification provider: {provider}.",
                nameof(provider));
        }

        if (string.IsNullOrWhiteSpace(providerReference))
        {
            throw new ArgumentException(
                "Provider reference is required.",
                nameof(providerReference));
        }

        if (string.IsNullOrWhiteSpace(bankCode))
        {
            throw new ArgumentException(
                "Bank code is required.",
                nameof(bankCode));
        }

        if (string.IsNullOrWhiteSpace(maskedAccountNumber))
        {
            throw new ArgumentException(
                "Masked account number is required.",
                nameof(maskedAccountNumber));
        }

        if (string.IsNullOrWhiteSpace(accountFingerprint))
        {
            throw new ArgumentException(
                "Account fingerprint is required.",
                nameof(accountFingerprint));
        }

        if (string.IsNullOrWhiteSpace(expectedAccountHolderName))
        {
            throw new ArgumentException(
                "Expected account holder name is required.",
                nameof(expectedAccountHolderName));
        }

        if (expiresAt.HasValue && expiresAt.Value <= createdAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAt),
                "Expiration time must be later than creation time.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider.Trim();
        ProviderReference = providerReference.Trim();
        BankCode = bankCode.Trim().ToUpperInvariant();
        MaskedAccountNumber = maskedAccountNumber.Trim();
        AccountFingerprint = accountFingerprint.Trim();
        ExpectedAccountHolderName =
            NormalizeAccountHolderName(expectedAccountHolderName);
        Status = BankAccountVerificationStatuses.Pending;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static BankAccountVerification CreatePending(
        Guid userId,
        string provider,
        string providerReference,
        string bankCode,
        string maskedAccountNumber,
        string accountFingerprint,
        string expectedAccountHolderName,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt)
    {
        return new BankAccountVerification(
            userId,
            provider,
            providerReference,
            bankCode,
            maskedAccountNumber,
            accountFingerprint,
            expectedAccountHolderName,
            createdAt,
            expiresAt);
    }

    public bool MarkVerified(
        string verifiedAccountHolderName,
        DateTimeOffset occurredAt)
    {
        if (Status == BankAccountVerificationStatuses.Verified)
        {
            return false;
        }

        if (Status != BankAccountVerificationStatuses.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot verify bank account in status {Status}.");
        }

        if (ExpiresAt.HasValue && occurredAt > ExpiresAt.Value)
        {
            Status = BankAccountVerificationStatuses.Expired;
            UpdatedAt = occurredAt;

            throw new InvalidOperationException(
                "Bank account verification request has expired.");
        }

        if (string.IsNullOrWhiteSpace(verifiedAccountHolderName))
        {
            throw new ArgumentException(
                "Verified account holder name is required.",
                nameof(verifiedAccountHolderName));
        }

        VerifiedAccountHolderName =
            NormalizeAccountHolderName(verifiedAccountHolderName);

        Status = BankAccountVerificationStatuses.Verified;
        VerifiedAt = occurredAt;
        FailureCode = null;
        FailureReason = null;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool Reject(
        string failureCode,
        string? failureReason,
        DateTimeOffset occurredAt)
    {
        if (Status == BankAccountVerificationStatuses.Rejected)
        {
            return false;
        }

        if (Status != BankAccountVerificationStatuses.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending bank verification can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            throw new ArgumentException(
                "Failure code is required.",
                nameof(failureCode));
        }

        Status = BankAccountVerificationStatuses.Rejected;
        FailureCode = failureCode.Trim();
        FailureReason = string.IsNullOrWhiteSpace(failureReason)
            ? null
            : failureReason.Trim();
        RejectedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool MarkExpired(DateTimeOffset occurredAt)
    {
        if (Status == BankAccountVerificationStatuses.Expired)
        {
            return false;
        }

        if (Status != BankAccountVerificationStatuses.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending bank verification can expire.");
        }

        Status = BankAccountVerificationStatuses.Expired;
        UpdatedAt = occurredAt;

        return true;
    }

    public bool Revoke(DateTimeOffset occurredAt)
    {
        if (Status == BankAccountVerificationStatuses.Revoked)
        {
            return false;
        }

        if (Status != BankAccountVerificationStatuses.Verified)
        {
            throw new InvalidOperationException(
                "Only a verified bank account can be revoked.");
        }

        Status = BankAccountVerificationStatuses.Revoked;
        RevokedAt = occurredAt;
        UpdatedAt = occurredAt;

        return true;
    }

    private static string NormalizeAccountHolderName(string name)
    {
        return string.Join(
                ' ',
                name.Trim()
                    .ToUpperInvariant()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries))
            .Trim();
    }
}