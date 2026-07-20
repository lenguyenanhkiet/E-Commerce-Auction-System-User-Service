using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.IdentityVerification
{
    public class IdentityVerification : AuditableEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }

        // Citizen identity card information
        public string FullName { get; private set; } = string.Empty;

        public string Gender { get; private set; } = string.Empty;
        public DateOnly DateOfBirth { get; private set; }

        public string IdentityNumber { get; private set; } = string.Empty;
        public DateOnly IssueDate { get; private set; }
        public DateOnly ExpiryDate { get; private set; }
        public string IssuePlace { get; private set; } = string.Empty;
        public string PermanentAddress { get; private set; } = string.Empty;

        public string IdentityFrontImageKey { get; private set; } = string.Empty;
        public string IdentityBackImageKey { get; private set; } = string.Empty;

        public string Status { get; private set; } = IdentityVerificationStatus.Pending;
        public string? RejectionReason { get; private set; }
        public decimal? ConfidenceScore { get; private set; }
        public DateTime SubmittedAt { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public string? VerifiedBy { get; private set; }

        protected IdentityVerification()
        { }

        public IdentityVerification(
         Guid userId,
         string fullName,
         string gender,
         DateOnly dateOfBirth,
         string identityNumber,
         DateOnly issueDate,
         DateOnly expiryDate,
         string issuePlace,
         string permanentAddress,
         string identityFrontImageKey,
         string identityBackImageKey)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required.", nameof(userId));
            }
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                throw new ArgumentException("Identity number is required.", nameof(identityNumber));
            }

            if (string.IsNullOrWhiteSpace(identityFrontImageKey) || string.IsNullOrWhiteSpace(identityBackImageKey))
            {
                throw new ArgumentException("Both front and back identity images are required.");
            }
            UserId = userId;
            FullName = fullName.Trim();
            Gender = gender.Trim();
            DateOfBirth = dateOfBirth;
            IdentityNumber = identityNumber.Trim();
            IssueDate = issueDate;
            ExpiryDate = expiryDate;
            IssuePlace = issuePlace.Trim();
            PermanentAddress = permanentAddress.Trim();
            IdentityFrontImageKey = identityFrontImageKey.Trim();
            IdentityBackImageKey = identityBackImageKey.Trim();
            Status = IdentityVerificationStatus.Pending;
            SubmittedAt = DateTime.UtcNow;
            CreatedAt = SubmittedAt;
            UpdatedAt = SubmittedAt;
        }

        /// <summary>
        /// Marks this verification as passed, based on the automated provider's result.
        /// </summary>
        public void Verify(decimal confidenceScore, string verifiedBy = "SYSTEM")
        {
            EnsureCanBeReviewed();
            Status = IdentityVerificationStatus.Verified;
            RejectionReason = null;
            ConfidenceScore = confidenceScore;
            VerifiedAt = DateTime.UtcNow;
            VerifiedBy = verifiedBy;
            UpdatedAt = VerifiedAt.Value;
        }

        /// <summary>
        /// Marks this verification as failed, based on the automated provider's result.
        /// </summary>
        public void Reject(string rejectionReason, string verifiedBy = "SYSTEM")
        {
            EnsureCanBeReviewed();
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new ArgumentException("Rejection reason is required.", nameof(rejectionReason));
            }
            Status = IdentityVerificationStatus.Rejected;
            RejectionReason = rejectionReason.Trim();
            VerifiedAt = DateTime.UtcNow;
            VerifiedBy = verifiedBy;
            UpdatedAt = VerifiedAt.Value;
        }

        /// <summary>
        /// Allows resubmitting new number/images after a rejection (e.g. blurry photo, wrong number).
        /// </summary>
        public void Resubmit(string fullName, string gender, DateOnly dateOfBirth, string identityNumber, DateOnly issueDate, DateOnly expiryDate, string issuePlace, string permanentAddress, string identityFrontImageKey, string identityBackImageKey)
        {
            if (Status != IdentityVerificationStatus.Rejected)
            {
                throw new InvalidOperationException("Only a rejected verification can be resubmitted.");
            }

            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                throw new ArgumentException("Identity number is required.", nameof(identityNumber));
            }

            if (string.IsNullOrWhiteSpace(identityFrontImageKey) || string.IsNullOrWhiteSpace(identityBackImageKey))
            {
                throw new ArgumentException("Both front and back identity images are required.");
            }
            FullName = fullName.Trim();
            Gender = gender.Trim();
            DateOfBirth = dateOfBirth;
            IdentityNumber = identityNumber.Trim();
            IssueDate = issueDate;
            ExpiryDate = expiryDate;
            IssuePlace = issuePlace.Trim();
            PermanentAddress = permanentAddress.Trim();
            IdentityFrontImageKey = identityFrontImageKey.Trim();
            IdentityBackImageKey = identityBackImageKey.Trim();
            Status = IdentityVerificationStatus.Pending;
            RejectionReason = null;
            ConfidenceScore = null;
            SubmittedAt = DateTime.UtcNow;
            UpdatedAt = SubmittedAt;
        }

        private void EnsureCanBeReviewed()
        {
            if (Status != IdentityVerificationStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot review a verification in status '{Status}'.");
            }
        }
    }
}