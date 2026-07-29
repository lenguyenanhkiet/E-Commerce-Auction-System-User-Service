using ECommerceAuction.UserService.Domain.Common;
using ECommerceAuction.UserService.Domain.Sellers.Applications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers;

public class SellerProfile : AuditableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string SellerType { get; private set; } = string.Empty;
    public string BusinessName { get; private set; } = string.Empty;
    public string ContactPhoneNumber { get; private set; } = string.Empty;
    public string TaxCode { get; private set; } = string.Empty;
    public string BusinessLicenseUrl { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string BankAccountNumber { get; private set; } = string.Empty;
    public string BankName { get; private set; } = string.Empty;
    public string BankAccountHolder { get; private set; } = string.Empty;
    public string Status { get; private set; } = SellerApplicationStatus.Pending;
    public string? RejectReason { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    private readonly List<SellerApplicationHistory> _history = new();
    public IReadOnlyCollection<SellerApplicationHistory> History => _history.AsReadOnly();

    protected SellerProfile()
    { }

    public SellerProfile(
    Guid userId,
    string sellerType,
    string businessName,
    string contactPhoneNumber,
    string taxCode,
    string businessLicenseUrl,
    string address,
    string bankAccountNumber,
    string bankName,
    string bankAccountHolder,
    DateTimeOffset submittedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessLicenseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankAccountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankAccountHolder);

        UserId = userId;
        SellerType = sellerType;
        BusinessName = businessName.Trim();
        ContactPhoneNumber = NormalizeContactPhoneNumber(contactPhoneNumber);
        TaxCode = NormalizeTaxCode(taxCode);
        BusinessLicenseUrl = businessLicenseUrl.Trim();
        Address = address.Trim();
        BankAccountNumber = bankAccountNumber.Trim();
        BankName = bankName.Trim();
        BankAccountHolder = bankAccountHolder.Trim();

        Status = SellerApplicationStatus.Pending;
        SubmittedAt = DateTimeOffset.UtcNow;
        CreatedAt = SubmittedAt;
        UpdatedAt = SubmittedAt;

        _history.Add(SellerApplicationHistory.Create(Id, null, Status, userId, "Application submitted."));
    }

    public static SellerProfile Create(
        Guid userId,
        string sellerType,
        string businessName,
        string contactPhoneNumber,
        string taxCode,
        string businessLicenseUrl,
        string address,
        string bankAccountNumber,
        string bankName,
        string bankAccountHolder,
        DateTimeOffset submittedAt)
    {
        return new SellerProfile(
            userId,
            sellerType,
            businessName,
            contactPhoneNumber,
            taxCode,
            businessLicenseUrl,
            address,
            bankAccountNumber,
            bankName,
            bankAccountHolder,
            submittedAt);
    }

    /// <summary>
    /// Moves the application into UnderReview so it shows up as "being worked on" by an admin.
    /// Optional — you can also approve/reject directly from Pending if you don't need this state.
    /// </summary>

    public void StartReview(Guid reviewerId, DateTimeOffset occurredAt)
    {
        EnsureValidReviewer(reviewerId);
        EnsureCanBeReviewed();

        var from = Status;
        Status = SellerApplicationStatus.UnderReview;
        ReviewedBy = reviewerId;
        UpdatedAt = occurredAt;

        _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, "Review started."));
    }

    public void Approve(Guid reviewerId, DateTimeOffset occurredAt)
    {
        EnsureValidReviewer(reviewerId);
        EnsureCanBeReviewed();

        var from = Status;
        Status = SellerApplicationStatus.Approved;
        RejectReason = null;
        ReviewedAt = occurredAt;
        ReviewedBy = reviewerId;
        UpdatedAt = occurredAt;

        _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, "Approved."));
    }

    public void Reject(Guid reviewerId, string reason, DateTimeOffset occurredAt)
    {
        EnsureValidReviewer(reviewerId);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reject reason is required.", nameof(reason));
        }

        EnsureCanBeReviewed();

        var from = Status;
        Status = SellerApplicationStatus.Rejected;
        RejectReason = reason.Trim();
        ReviewedAt = occurredAt;
        ReviewedBy = reviewerId;
        UpdatedAt = occurredAt;

        _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, reason.Trim()));
    }

    /// <summary>
    /// Allows a user to edit and resubmit a Rejected application, sending it back to Pending.
    /// </summary>
    public void Resubmit(
        string businessName,
        string contactPhoneNumber,
        string taxCode,
        string businessLicenseUrl,
        string address,
        string bankAccountNumber,
        string bankName,
        string bankAccountHolder,
        DateTimeOffset submittedAt)
    {
        if (Status != SellerApplicationStatus.Rejected)
        {
            throw new InvalidOperationException("Only a rejected application can be resubmitted.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessLicenseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankAccountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankAccountHolder);

        BusinessName = businessName.Trim();
        ContactPhoneNumber =
            NormalizeContactPhoneNumber(contactPhoneNumber);
        TaxCode = NormalizeTaxCode(taxCode);
        BusinessLicenseUrl = businessLicenseUrl.Trim();
        Address = address.Trim();
        BankAccountNumber = bankAccountNumber.Trim();
        BankName = bankName.Trim();
        BankAccountHolder = bankAccountHolder.Trim();

        var from = Status;
        Status = SellerApplicationStatus.Pending;
        RejectReason = null;
        ReviewedAt = null;
        ReviewedBy = null;
        SubmittedAt = submittedAt;
        UpdatedAt = submittedAt;

        _history.Add(SellerApplicationHistory.Create(Id, from, Status, UserId, "Resubmitted after rejection."));
    }

    private void EnsureCanBeReviewed()
    {
        if (Status is not (SellerApplicationStatus.Pending or SellerApplicationStatus.UnderReview))
        {
            throw new InvalidOperationException(
                $"Cannot review an application in status '{Status}'.");
        }
    }

    private static void EnsureValidReviewer(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reviewer ID cannot be empty.",
                nameof(reviewerId));
        }
    }

    private static string NormalizeTaxCode(string taxCode)
    {
        return taxCode.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }

    private static string NormalizeContactPhoneNumber(string phoneNumber)
    {
        var normalized = phoneNumber.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
        if (normalized.StartsWith("+84", StringComparison.Ordinal))
        {
            normalized = $"0{normalized[3..]}";
        }
        if (normalized.Length is < 9 or > 11 ||
            !normalized.All(char.IsDigit))
        {
            throw new ArgumentException(
                "Contact phone number is invalid.",
                nameof(phoneNumber));
        }

        return normalized;
    }
}