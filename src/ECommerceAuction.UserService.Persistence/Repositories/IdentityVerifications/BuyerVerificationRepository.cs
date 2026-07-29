using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.IdentityVerifications;

public sealed class BuyerVerificationRepository : IBuyerVerificationRepository
{
    private readonly ApplicationDbContext _context;

    public BuyerVerificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<BuyerVerificationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.BuyerVerificationProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.BuyerVerificationProfiles
            .AnyAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(
        BuyerVerificationProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _context.BuyerVerificationProfiles.AddAsync(profile, cancellationToken);
    }
}
