using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Domain.Repositories;

/// <summary>
/// Đạt + Tùng: repository contract for local register/login and OAuth account lookup operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Checks whether an active email already exists in SQL.
    /// </summary>
    Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an active phone number already exists in SQL.
    /// </summary>
    Task<bool> CheckPhoneExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a verified user to the current unit of work.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the user's default reputation profile to the current unit of work.
    /// </summary>
    Task AddReputationProfileAsync(
        ReputationProfile profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email or phone number for local login.
    /// </summary>
    Task<User?> GetByEmailOrPhoneAsync(
        string emailOrPhone,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active role codes assigned to a user.
    /// </summary>
    Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
