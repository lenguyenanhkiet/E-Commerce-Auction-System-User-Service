using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Sellers
{
    public class SellerProfile : AuditableEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public string SellerType { get; private set; } = string.Empty;
        public string BusinessName { get; private set; } = string.Empty;
        public string TaxCode { get; private set; } = string.Empty;
        public string BusinessLicenseUrl  { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string BankAccountNumber { get; private set; } = string.Empty;
        public string BankName { get; private set; } = string.Empty;
        public string BankAccountHolder { get; private set; } = string.Empty;
        public string Status { get; private set; } = SellerApplicationStatus.Pending;
        public string? RejectReason { get; private set; }
        public DateTime SubmittedAt { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public Guid? ReviewedBy { get; private set; }
        private readonly List<SellerApplicationHistory> _history = new();
        public IReadOnlyCollection<SellerApplicationHistory> History => _history.AsReadOnly();

        protected SellerProfile() {}
        public SellerProfile(
        Guid userId,
        string sellerType,
        string businessName,
        string taxCode,
        string businessLicenseUrl,
        string address,
        string bankAccountNumber,
        string bankName,
        string bankAccountHolder)
        {
            UserId = userId;
            SellerType = sellerType;
            BusinessName = businessName;
            TaxCode = taxCode;
            BusinessLicenseUrl = businessLicenseUrl;
            Address = address.Trim();
            BankAccountNumber = bankAccountNumber.Trim();
            BankName = bankName.Trim();
            BankAccountHolder = bankAccountHolder.Trim();

            Status = Sellers.SellerApplicationStatus.Pending;
            SubmittedAt = DateTime.UtcNow;
            CreatedAt = SubmittedAt;
            UpdatedAt = SubmittedAt;

            _history.Add(SellerApplicationHistory.Create(Id, null, Status, userId, "Application submitted."));

        }
        /// <summary>
        /// Moves the application into UnderReview so it shows up as "being worked on" by an admin.
        /// Optional — you can also approve/reject directly from Pending if you don't need this state.
        /// </summary>

        public void StartReview(Guid reviewerId)
        {
            EnsureCanBeReviewed();

            var from = Status;
            Status = Sellers.SellerApplicationStatus.UnderReview;
            UpdatedAt = DateTime.UtcNow;

            _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, "Review started."));
        }

        public void Approve(Guid reviewerId)
        {
            EnsureCanBeReviewed();

            var from = Status;
            Status = Sellers.SellerApplicationStatus.Approved;
            RejectReason = null;
            ReviewedAt = DateTime.UtcNow;
            ReviewedBy = reviewerId;
            UpdatedAt = ReviewedAt.Value;

            _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, "Approved."));
        }

        public void Reject(Guid reviewerId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Reject reason is required.", nameof(reason));
            }

            EnsureCanBeReviewed();

            var from = Status;
            Status = Sellers.SellerApplicationStatus.Rejected;
            RejectReason = reason.Trim();
            ReviewedAt = DateTime.UtcNow;
            ReviewedBy = reviewerId;
            UpdatedAt = ReviewedAt.Value;

            _history.Add(SellerApplicationHistory.Create(Id, from, Status, reviewerId, reason.Trim()));
        }

        /// <summary>
        /// Allows a user to edit and resubmit a Rejected application, sending it back to Pending.
        /// </summary>
        public void Resubmit(
            string businessName,
            string taxCode,
            string businessLicenseUrl,
            string address,
            string bankAccountNumber,
            string bankName,
            string bankAccountHolder)
        {
            if (Status != Sellers.SellerApplicationStatus.Rejected)
            {
                throw new InvalidOperationException("Only a rejected application can be resubmitted.");
            }

            BusinessName = businessName.Trim();
            TaxCode = taxCode.Trim();
            BusinessLicenseUrl = businessLicenseUrl;
            Address = address.Trim();
            BankAccountNumber = bankAccountNumber.Trim();
            BankName = bankName.Trim();
            BankAccountHolder = bankAccountHolder.Trim();

            var from = Status;
            Status = Sellers.SellerApplicationStatus.Pending;
            RejectReason = null;
            SubmittedAt = DateTime.UtcNow;
            UpdatedAt = SubmittedAt;

            _history.Add(SellerApplicationHistory.Create(Id, from, Status, UserId, "Resubmitted after rejection."));
        }

        private void EnsureCanBeReviewed()
        {
            if (Status is not (Sellers.SellerApplicationStatus.Pending or Sellers.SellerApplicationStatus.UnderReview))
            {
                throw new InvalidOperationException(
                    $"Cannot review an application in status '{Status}'.");
            }
        }
    }
}

