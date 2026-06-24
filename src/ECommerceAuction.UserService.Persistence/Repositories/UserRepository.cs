using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Domain.Entities.Users;
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

	public async Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();

		return await _context.Users
			.AnyAsync(x => x.Email == normalizedEmail && x.DeletedAt == null, cancellationToken);
	}

	public async Task<bool> CheckPhoneExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
	{
		var normalizedPhoneNumber = phoneNumber.Trim();

		return await _context.Users
			.AnyAsync(x => x.PhoneNumber == normalizedPhoneNumber && x.DeletedAt == null, cancellationToken);
	}

	public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default)
	{
		// Đạt - Local Login: support email or phone in the same login field.
		var normalizedInput = emailOrPhone.Trim();
		var normalizedEmail = normalizedInput.ToLowerInvariant();

		return await _context.Users
			.FirstOrDefaultAsync(
				x => (x.Email == normalizedEmail || x.PhoneNumber == normalizedInput)
					 && x.DeletedAt == null,
				cancellationToken);
	}


    public async Task<List<string>> GetUserRolesAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(x => x.UserId == userId
                && x.Status == "ACTIVE"
                && x.RevokedAt == null
                && x.Role.Status == "ACTIVE"
                && x.Role.DeletedAt == null)
            .Select(x => x.Role.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task AddReputationProfileAsync(
    ReputationProfile profile,
    CancellationToken cancellationToken = default)
    {
        await _context.ReputationProfiles.AddAsync(profile, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId && x.DeletedAt == null,
                cancellationToken);
    }
}
