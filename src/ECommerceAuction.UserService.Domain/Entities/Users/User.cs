using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Entities.Users;

public class User : AuditableEntity, IAggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? IdentityNumber { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Gender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Address { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime? PasswordChangedAt { get; private set; }

    public string Status { get; private set; } = UserStatus.Active;
    public bool IsEmailConfirmed { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public ReputationProfile? ReputationProfile { get; private set; }
    // CONSTRUCTORS
    protected User() { }

    // Constructor used for Local Registration (After OTP verification on Redis has been completed)
    public User(string email, string passwordHash, string fullName, string? phoneNumber)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;

        // Since you've already passed the Redis OTP step, your account is definitely valid by now.
        Status = UserStatus.Active;
        IsEmailConfirmed = true;
        IsPhoneConfirmed = false;
        FailedLoginAttempts = 0;
    }

    // 3. Factory Method creates User automatically from Google Login flow (OAuth2 by Tung)
    public static User CreateFromOAuth2(string email, string fullName)
    {
        return new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = string.Empty, // The Google account does not have a system password.
            Status = UserStatus.Active,
            IsEmailConfirmed = true,     // Google has already verified the email.
            FailedLoginAttempts = 0
        };
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public void ResetFailedLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;
    }

    public void AssignRole(UserRole role)
    {
        if (_userRoles.Any(r => r.RoleId == role.RoleId))
            return;

        _userRoles.Add(role);
    }
}