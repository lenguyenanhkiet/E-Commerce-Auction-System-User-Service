using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Repositories.IdentityVerifications
{
    public class IdentityVerificationRepository : IIdentityVerificationRepository
    {
        private readonly ApplicationDbContext _context;
        public IdentityVerificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(IdentityVerification identityVerification, CancellationToken cancellationToken)
        {
            await _context.IdentityVerifications.AddAsync(identityVerification, cancellationToken);
        }

        public async Task<IdentityVerification?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.IdentityVerifications
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<IReadOnlyList<IdentityVerification>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken)
        {
            if (userIds.Count == 0)
            {
                return Array.Empty<IdentityVerification>();
            }

            return await _context.IdentityVerifications
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);
        }
    }
}
