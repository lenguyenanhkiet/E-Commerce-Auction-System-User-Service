using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Domain.Entities.Roles;

/// <summary>
/// Represents an RBAC role and protects role lifecycle rules inside the domain.
/// </summary>
public sealed class Role
{
    private readonly List<RolePrivilege> _rolePrivileges = [];

    private Role()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public string Status { get; private set; } = RoleStatuses.Active;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public IReadOnlyCollection<RolePrivilege> RolePrivileges => _rolePrivileges;

    /// <summary>
    /// Creates a custom role that administrators may update or delete through Role Management.
    /// </summary>
    public static Role CreateCustom(string code, string name, string? description)
    {
        return Create(code, name, description, isSystemRole: false);
    }

    /// <summary>
    /// Creates a system-owned role such as the default BUYER role.
    /// </summary>
    public static Role CreateSystemRole(string code, string name, string? description)
    {
        return Create(code, name, description, isSystemRole: true);
    }

    /// <summary>
    /// Updates mutable role information while keeping the role code stable for authorization references.
    /// </summary>
    public void UpdateInfo(string name, string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deletes a role so audit and historical references remain available.
    /// </summary>
    public void MarkDeleted()
    {
        Status = RoleStatuses.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static Role Create(
        string code,
        string name,
        string? description,
        bool isSystemRole)
    {
        var now = DateTimeOffset.UtcNow;

        return new Role
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = NormalizeDescription(description),
            IsSystemRole = isSystemRole,
            Status = RoleStatuses.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}

/// <summary>
/// Defines default role codes used by User Service.
/// </summary>
public static class RoleCodes
{
    public const string Admin = "ADMIN";
    public const string Buyer = "BUYER";
    public const string Seller = "SELLER";
    public const string SupportStaff = "SUPPORT_STAFF";
}

/// <summary>
/// Defines role lifecycle statuses.
/// </summary>
public static class RoleStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Deleted = "DELETED";
}
