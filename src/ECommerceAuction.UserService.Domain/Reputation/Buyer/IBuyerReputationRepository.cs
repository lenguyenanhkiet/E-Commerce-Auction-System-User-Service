using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Reputation.Buyer;

public interface IBuyerReputationRepository
{
    Task<BuyerReputationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BuyerReputationProfile profile,
        CancellationToken cancellationToken = default);
}
