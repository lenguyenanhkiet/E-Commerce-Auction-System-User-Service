using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Domain.Repositories;

/// <summary>
/// Repository contract for local register/login and OAuth account lookup operations.
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

    /// <summary>
    /// Gets a user by id for the current user's profile.
    /// </summary>
    Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an email belongs to another active user.
    /// </summary>
    Task<bool> CheckEmailExistsForOtherUserAsync(
        string email,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    Task<User?> GetByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(
    string email,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a password reset token.
    /// </summary>
    Task AddPasswordResetTokenAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a password reset token.
    /// </summary>
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a user audit log entry.
    /// </summary>
    Task AddAuditLogAsync(
        UserAuditLog auditLog,
        CancellationToken cancellationToken = default);



    /// <summary>
    /// Gets a paginated user list for the Admin management page.
    /// </summary>
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        string? search,
        string? gender,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
