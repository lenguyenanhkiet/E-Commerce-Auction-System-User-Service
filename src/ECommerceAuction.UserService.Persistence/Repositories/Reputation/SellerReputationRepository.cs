using ECommerceAuction.UserService.Domain.Reputation.Seller;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Reputation;

public sealed class SellerReputationRepository : ISellerReputationRepository
{
    private readonly ApplicationDbContext _context;

    public SellerReputationRepository(ApplicationDbContext context) =>
        _context = context;

    public Task<SellerReputationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _context.SellerReputationProfiles.FirstOrDefaultAsync(
            profile => profile.UserId == userId,
            cancellationToken);

    public async Task AddAsync(
        SellerReputationProfile profile,
        CancellationToken cancellationToken = default) =>
        await _context.SellerReputationProfiles.AddAsync(
            profile,
            cancellationToken);
}
