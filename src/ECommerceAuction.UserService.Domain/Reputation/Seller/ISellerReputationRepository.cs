namespace ECommerceAuction.UserService.Domain.Reputation.Seller;

public interface ISellerReputationRepository
{
    Task<SellerReputationProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SellerReputationProfile profile,
        CancellationToken cancellationToken = default);
}
