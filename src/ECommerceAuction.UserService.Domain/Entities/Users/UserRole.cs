using ECommerceAuction.UserService.Domain.Entities.Roles;

namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Represents an assignment between a user and an RBAC role.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string Status { get; set; } = UserRoleStatuses.Active;
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Creates an active user-role assignment.
    /// </summary>
    public static UserRole Assign(Guid userId, Guid roleId, Guid? assignedBy = null)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            Status = UserRoleStatuses.Active
        };
    }
}

/// <summary>
/// Defines lifecycle statuses for user-role assignments.
/// </summary>
public static class UserRoleStatuses
{
    public const string Active = "ACTIVE";
    public const string Revoked = "REVOKED";
}
