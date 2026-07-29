using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.IdentityVerifications;

public interface IBuyerVerificationRepository
{
    Task<BuyerVerificationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BuyerVerificationProfile profile,
        CancellationToken cancellationToken = default);
}