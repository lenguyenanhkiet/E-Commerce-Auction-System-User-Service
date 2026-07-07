using ECommerceAuction.UserService.Domain.Entities.Reputation;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckEmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(
                user =>
                    user.Email == normalizedEmail &&
                    user.DeletedAt == null,
                cancellationToken);
    }

    public async Task<bool> CheckPhoneExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhoneNumber = phoneNumber.Trim();

        return await _context.Users
            .AnyAsync(
                user =>
                    user.PhoneNumber == normalizedPhoneNumber &&
                    user.DeletedAt == null,
                cancellationToken);
    }


    public async Task<User?> GetByEmailOrPhoneAsync(
        string emailOrPhone,
        CancellationToken cancellationToken = default)
    {
        // Support email or phone number in the same login field.
        var normalizedInput = emailOrPhone.Trim();
        var normalizedEmail = normalizedInput.ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(
                user =>
                    (user.Email == normalizedEmail ||
                     user.PhoneNumber == normalizedInput) &&
                    user.DeletedAt == null,
                cancellationToken);
    }

    public async Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(userRole =>
                userRole.UserId == userId &&
                userRole.Status == "ACTIVE" &&
                userRole.RevokedAt == null &&
                userRole.Role.Status == "ACTIVE" &&
                userRole.Role.DeletedAt == null)
            .Select(userRole => userRole.Role.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task AddReputationProfileAsync(
        ReputationProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _context.ReputationProfiles.AddAsync(
            profile,
            cancellationToken);
    }

    /// <summary>
    /// Gets a user by id for the current user's profile.
    /// </summary>
    public async Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(user => user.UserRoles)
            .Include(user => user.ReputationProfile)
            .FirstOrDefaultAsync(
                user =>
                    user.Id == userId &&
                    user.DeletedAt == null,
                cancellationToken);
    }

    /// <summary>
    /// Checks whether an email belongs to another active user.
    /// </summary>
    public async Task<bool> CheckEmailExistsForOtherUserAsync(
        string email,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        return await _context.Users
            .AnyAsync(
                user =>
                    user.Email == normalizedEmail &&
                    user.Id != userId &&
                    user.DeletedAt == null,
                cancellationToken);
    }

    /// <summary>
    /// Gets a paginated user list for the Admin management page.
    /// </summary>
    public async Task<(IReadOnlyList<User> Items, int TotalCount)>
        GetPagedAsync(
            string? search,
            string? gender,
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        // The query only reads data, so EF Core does not need to track entities.
        IQueryable<User> query = _context.Users
            .AsNoTracking()
            .Where(user => user.DeletedAt == null);

        // Search by full name, email, or phone number.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(user =>
                user.FullName.Contains(keyword) ||
                user.Email.Contains(keyword) ||
                user.PhoneNumber.Contains(keyword));
        }

        // Filter by gender.
        if (!string.IsNullOrWhiteSpace(gender))
        {
            var normalizedGender = gender
                .Trim()
                .ToUpperInvariant();

            query = query.Where(user =>
                user.Gender != null &&
                user.Gender.ToUpper() == normalizedGender);
        }

        // Filter by user status.
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status
                .Trim()
                .ToUpperInvariant();

            query = query.Where(user =>
                user.Status == normalizedStatus);
        }

        // Count all matching records before pagination.
        var totalCount = await query.CountAsync(cancellationToken);

        // Default sorting is by full name.
        // Id is used as the second sorting condition for stable pagination.
        var users = await query
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }


    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    public async Task<User?> GetByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == normalizedEmail && x.DeletedAt == null,
                cancellationToken);
    }

    /// <summary>
    /// Adds a password reset token to the database.
    /// </summary>
    public async Task AddPasswordResetTokenAsync(
    PasswordResetToken token,
    CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    /// <summary>
    /// Retrieves a password reset token by its value.
    /// </summary>
    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(
    string token,
    CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(
                x => x.Token == token,
                cancellationToken);
    }

    /// <summary>
    /// Adds a user audit log entry to the database.
    /// </summary>
    public async Task AddAuditLogAsync(
    UserAuditLog auditLog,
    CancellationToken cancellationToken = default)
    {
        await _context.UserAuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
