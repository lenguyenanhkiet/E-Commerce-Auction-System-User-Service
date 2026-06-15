namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Represents an RBAC role assigned to users.
/// </summary>
public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public string Status { get; set; } = RoleStatuses.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Creates a system-owned role such as the default BUYER role.
    /// </summary>
    public static Role CreateSystemRole(string code, string name, string? description)
    {
        return new Role
        {
            Code = code,
            Name = name,
            Description = description,
            IsSystemRole = true,
            Status = RoleStatuses.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Defines default role codes used by User Service.
/// </summary>
public static class RoleCodes
{
    public const string Buyer = "BUYER";
}

/// <summary>
/// Defines role lifecycle statuses.
/// </summary>
public static class RoleStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}
