using ECommerceAuction.UserService.Domain.Common;
using ECommerceAuction.UserService.Domain.Reputation;
using System.ComponentModel.DataAnnotations;

namespace ECommerceAuction.UserService.Domain.Users;

/// <summary>
/// Represents an application user owned by User Service.
/// </summary>
public class User : AuditableEntity, IAggregateRoot
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; private set; } = string.Empty;

    [Phone]
    [StringLength(12)]
    public string PhoneNumber { get; private set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; private set; } = string.Empty;

    [StringLength(10)]
    public string? Gender { get; private set; }

    public DateOnly? DateOfBirth { get; private set; }

    [StringLength(500)]
    public string? AvatarUrl { get; private set; }

    [StringLength(500)]
    public string? AvatarKey { get; private set; }

    [Required]
    public string PasswordHash { get; private set; } = string.Empty;

    public bool MustChangePassword { get; private set; }
    public DateTime? PasswordChangedAt { get; private set; }

    public string Status { get; private set; } = UserStatus.Active;
    public bool IsEmailConfirmed { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }
    public bool IsIdentityVerified { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? StatusExpiresAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    [StringLength(20)]
    public string AuthProvider { get; private set; } = AuthProviders.Local;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    public ReputationProfile? ReputationProfile { get; private set; }

    protected User()
    {
    }

    /// <summary>
    /// Creates a local account after the registration flow has validated the required data.
    /// </summary>
    public User(string email, string passwordHash, string fullName, string? phoneNumber)
    {
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Status = UserStatus.Active;
        IsEmailConfirmed = true;
        IsPhoneConfirmed = false;
        FailedLoginAttempts = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        // The account is created with a password, so its age starts now.
        PasswordChangedAt = CreatedAt;
    }

    /// <summary>
    /// Creates a local account with a pre-generated id from the pending registration cache.
    /// </summary>
    public User(Guid id, string email, string passwordHash, string fullName, string? phoneNumber)
        : this(email, passwordHash, fullName, phoneNumber)
    {
        // Keep the final SQL user id identical to the pending registration id.
        Id = id;
    }

    /// <summary>
    /// Creates a local account from a verified Google OAuth2 identity.
    /// </summary>
    public static User CreateFromOAuth2(string email, string fullName)
    {
        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = string.IsNullOrWhiteSpace(fullName) ? email.Trim().ToLowerInvariant() : fullName.Trim(),
            PasswordHash = string.Empty,
            Status = UserStatus.Active,
            IsEmailConfirmed = true,
            FailedLoginAttempts = 0,
            AuthProvider = AuthProviders.Google,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a local account by an administrator with the full set of profile fields.
    /// Admin-created accounts are trusted, so the email is marked as confirmed.
    /// </summary>
    public static User CreateByAdmin(
        string email,
        string passwordHash,
        string fullName,
        string phoneNumber,
        string? gender,
        DateOnly? dateOfBirth
        )
    {
        var user = new User(email, passwordHash, fullName, phoneNumber)
        {
            Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim(),
            DateOfBirth = dateOfBirth,
        };
        return user;
    }

    /// <summary>
    /// Updates the editable profile fields on behalf of an administrator.
    /// Email is handled separately through <see cref="ChangeEmailByAdmin"/>.
    /// </summary>
    public void AdminUpdate(
        string fullName,
        string? gender,
        DateOnly? dateOfBirth
        )
    {
        FullName = fullName.Trim();
        Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies an email change performed by an administrator directly, without a verification round-trip.
    /// </summary>
    public void ChangeEmailByAdmin(string newEmail)
    {
        Email = newEmail.Trim().ToLowerInvariant();
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the account as soft-deleted so it is excluded from active queries.
    /// </summary>
    public void AdminSoftDeleteUser()
    {
        Status = UserStatus.Banned;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a failed local login attempt and temporarily locks the account after repeated failures.
    /// </summary>
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            Status = UserStatus.Locked;
            StatusExpiresAt = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears failed login counters after a successful authentication.
    /// </summary>
    public void ResetFailedLogin()
    {
        FailedLoginAttempts = 0;
        StatusExpiresAt = null;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = UserStatus.Active;
    }

    /// <summary>
    /// Marks this user as successfully logged in by any authentication method.
    /// </summary>
    public void MarkLoggedIn()
    {
        ResetFailedLogin();
    }

    /// <summary>
    /// Updates the user's editable profile fields.
    /// Email change is intentionally NOT applied here — it must go through
    /// the email-verification flow in Notification Service first.
    /// </summary>
    public void UpdateProfile(string phoneNumber)
    {
        var newPhoneNumber = phoneNumber.Trim();

        // Changing to a different phone number invalidates the previous OTP verification.
        if (!string.Equals(PhoneNumber, newPhoneNumber, StringComparison.Ordinal))
        {
            IsPhoneConfirmed = false;
        }

        PhoneNumber = newPhoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Called after Notification Service confirms email ownership via the
    /// verify-email-change link. Applies the new email value directly.
    /// </summary>
    public void ConfirmEmailChange(string newEmail)
    {
        Email = newEmail.Trim().ToLowerInvariant();
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmPhoneChange()
    {
        IsPhoneConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the user's avatar image URL and its storage key after a successful upload.
    /// Passing null/empty clears both.
    /// </summary>
    public void SetAvatar(string? avatarUrl, string? avatarKey)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        AvatarKey = string.IsNullOrWhiteSpace(avatarKey) ? null : avatarKey.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a role assignment if the role is not already assigned.
    /// </summary>
    public void AssignRole(UserRole role)
    {
        if (_userRoles.Any(r => r.RoleId == role.RoleId && r.RevokedAt is null))
        {
            return;
        }

        _userRoles.Add(role);
    }

    public void AddAddress(Address address)
    {
        if (address.UserId != Id)
        {
            throw new InvalidOperationException("Address must belong to this user.");
        }
        _addresses.Add(address);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the default address for this user
    /// </summary>
    /// <returns>Address IsDefault</returns>
    public Address? GetDefaultAddress()
    {
        return _addresses.FirstOrDefault(a => a.IsDefault && a.DeletedAt == null);
    }

    public IReadOnlyCollection<Address> GetActiveAddresses()
    {
        return _addresses.Where(a => a.DeletedAt == null).ToList().AsReadOnly();
    }

    /// <summary>
    /// Checks whether the user can receive tokens after a successful identity verification.
    /// </summary>
    public bool CanLogin()
    {
        if (DeletedAt is not null)
        {
            return false;
        }

        if (Status == UserStatus.Banned)
        {
            return false;
        }

        if (Status == UserStatus.Locked && StatusExpiresAt > DateTime.UtcNow)
        {
            return false;
        }

        return Status is UserStatus.Active or UserStatus.Restricted or UserStatus.Locked;
    }

    /// <summary>
    /// Checks hard authentication blocks that Google OAuth2 must not bypass.
    /// </summary>
    public bool IsBlockedFromAuthentication()
    {
        return DeletedAt is not null ||
               Status == UserStatus.Banned ||
               (Status == UserStatus.Locked && StatusExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Marks the user's email as verified because Google has verified that email ownership.
    /// </summary>
    public void VerifyEmailByGoogle()
    {
        // ECA-6 OAuth2 Google: Google id_token has a verified email claim, so this local email can be trusted.
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears an unsafe password from an unverified local account that Google has safely claimed.
    /// </summary>
    public void ClearPasswordAfterUnverifiedGoogleClaim()
    {
        // ECA-6 OAuth2 Google: prevent account takeover when someone pre-created this email with an arbitrary password.
        PasswordHash = string.Empty;
        MustChangePassword = false;
        PasswordChangedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MustChangePassword = false;
        PasswordChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void FlagMustChangePassword()
    {
        MustChangePassword = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyIdentity(string legalFullName, string legelGender, DateOnly legalDateOfBirth)
    {
        IsIdentityVerified = true;
        FullName = legalFullName;
        Gender = legelGender;
        DateOfBirth = legalDateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }
}