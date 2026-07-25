using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.IdentityVerifications
{
    public interface IIdentityVerificationRepository
    {
        Task<IdentityVerification?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<IdentityVerification>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

        Task AddAsync(IdentityVerification identityVerification, CancellationToken cancellationToken);
    }
}