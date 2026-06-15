using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Domain.Repositories;

public interface IUserRepository
{
    // ĐẠT VÀ TÙNG ĐỊNH NGHĨA HÀM DB CẦN DÙNG
    Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> CheckPhoneExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task AddReputationProfileAsync(
      ReputationProfile profile,
      CancellationToken cancellationToken = default);

    Task<User?> GetByEmailOrPhoneAsync(
    string emailOrPhone,
    CancellationToken cancellationToken = default);

    Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}