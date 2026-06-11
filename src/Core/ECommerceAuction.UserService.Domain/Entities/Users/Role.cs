namespace ECommerceAuction.UserService.Domain.Entities.Users;

public class Role
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}