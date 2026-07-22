namespace ECommerceAuction.UserService.Domain.Entities.Roles;

/// <summary>
/// Represents a fine-grained action that can be granted to an RBAC role.
/// </summary>
public sealed class Privilege
{
    private Privilege()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Status { get; private set; } = PrivilegeStatuses.Active;

    /// <summary>
    /// Creates a system-defined privilege used by RBAC seed data and authorization policies.
    /// </summary>
    public static Privilege CreateSystem(string code, string name, string? description)
    {
        return new Privilege
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = PrivilegeStatuses.Active
        };
    }
}

/// <summary>
/// Defines privilege lifecycle statuses.
/// </summary>
public static class PrivilegeStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}