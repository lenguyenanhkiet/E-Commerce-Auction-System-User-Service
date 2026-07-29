using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Sellers
{
    public interface ISellerProfileRepository
    {
        Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a user's most recent seller application, if any.
        /// </summary>
        Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// True if the user already has a Pending, UnderReview, or Approved application.
        /// </summary>
        Task<bool> HasActiveApplicationAsync(Guid userId, CancellationToken cancellationToken);

        Task AddAsync(SellerProfile sellerProfile, CancellationToken cancellationToken);

        Task<(IReadOnlyList<SellerProfile> Items, int TotalCount)> GetPagedAsync(
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
    }
}