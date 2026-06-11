namespace ECommerceAuction.UserService.Domain.Entities.Users;

public class User
{
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>(); 

    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? IdentityNumber { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime? PasswordChangedAt { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public bool EmailVerified { get; set; }

    public bool PhoneVerified { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}