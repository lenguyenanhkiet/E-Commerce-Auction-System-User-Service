using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Reputation;

public sealed class BuyerReputationRepository : IBuyerReputationRepository
{
    private readonly ApplicationDbContext _context;

    public BuyerReputationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<BuyerReputationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.BuyerReputationProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(
        BuyerReputationProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _context.BuyerReputationProfiles.AddAsync(profile, cancellationToken);
    }
}
