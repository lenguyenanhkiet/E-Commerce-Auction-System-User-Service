using ECommerceAuction.UserService.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Entities.Users
{
    public class Address : AuditableEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }

        [Required]
        [StringLength(200)]
        public string RecipientName { get; private set; } = string.Empty;

        [Required]
        [StringLength(12)]
        public string RecipientPhone { get; private set; } = string.Empty;

        [StringLength(50)]
        public string? Province { get; private set; }

        [StringLength(50)]
        public string? Ward { get; private set; }

        [Required]
        [StringLength(255)]
        public string Street { get; private set; } = string.Empty;

        // Home, Work, Other
        [StringLength(50)]
        public string? Type { get; private set; }

        public bool IsDefault { get; private set; } = false;

        public User User { get; private set; } = null!;

        protected Address()
        { }

        public Address(
            Guid userId,
            string recipientName,
            string recipientPhone,
            string province,
            string ward,
            string street,
            string type,
            bool isDefault = false)
        {
            UserId = userId;
            RecipientName = recipientName.Trim();
            RecipientPhone = recipientPhone.Trim();
            Province = string.IsNullOrWhiteSpace(province) ? null : province.Trim(); ;
            Ward = string.IsNullOrWhiteSpace(ward) ? null : ward.Trim();
            Street = street.Trim();
            Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim(); ;
            IsDefault = isDefault;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Update Address information
        /// </summary>
        public void Update
            (
            string recipientName,
            string recipientPhone,
            string street,
            string? province = null,
            string? ward = null,
            string? type = null
            )
        {
            RecipientName = recipientName.Trim();
            RecipientPhone = recipientPhone.Trim();
            Province = string.IsNullOrWhiteSpace(province) ? null : province.Trim(); ;
            Ward = string.IsNullOrWhiteSpace(ward) ? null : ward.Trim();
            Street = street.Trim();
            Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UnsetAsDefault()
        {
            IsDefault = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}