using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers.Applications
{
    /// <summary>
    /// Audit trail entry for a seller application status change.
    /// </summary>
    public class SellerApplicationHistory : AuditableEntity
    {
        public Guid SellerProfileId { get; private set; }
        public string? FromStatus { get; private set; }
        public string ToStatus { get; private set; } = string.Empty;
        public Guid ChangedBy { get; private set; }
        public string? Note { get; private set; }
        public DateTimeOffset ChangedAt { get; private set; }

        protected SellerApplicationHistory()
        { }

        private SellerApplicationHistory(
        Guid sellerProfileId,
        string? fromStatus,
        string toStatus,
        Guid changedBy,
        string? note)
        {
            SellerProfileId = sellerProfileId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedBy = changedBy;
            Note = note;
            ChangedAt = DateTimeOffset.UtcNow;
            CreatedAt = ChangedAt;
            UpdatedAt = ChangedAt;
        }

        public static SellerApplicationHistory Create(
            Guid sellerProfileId,
            string? fromStatus,
            string toStatus,
            Guid changedBy,
            string? note)
        {
            return new SellerApplicationHistory(sellerProfileId, fromStatus, toStatus, changedBy, note);
        }
    }
}