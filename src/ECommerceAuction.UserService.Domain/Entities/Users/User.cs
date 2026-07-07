using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Represents an application user owned by User Service.
/// </summary>
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Records a failed local login attempt and temporarily locks the account after repeated failures.
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears failed login counters after a successful authentication.
    /// </summary>
    public void ResetFailedLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
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
    public void UpdateProfile(string phoneNumber, string address)
    {
        PhoneNumber = phoneNumber.Trim();
        Address = address.Trim();
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

        if (Status == UserStatus.Locked && LockedUntil > DateTime.UtcNow)
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
               (Status == UserStatus.Locked && LockedUntil > DateTime.UtcNow);
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
        PasswordChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
