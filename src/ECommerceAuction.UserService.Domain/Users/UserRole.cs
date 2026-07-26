using ECommerceAuction.UserService.Domain.Entities.Roles;

namespace ECommerceAuction.UserService.Domain.Users;

/// <summary>
/// Represents an assignment between a user and an RBAC role.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
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
            AssignedAt = DateTimeOffset.UtcNow,
            Status = UserRoleStatuses.Active
        };
    }

    /// <summary>
    /// Revokes an active assignment so the role no longer grants access.
    /// </summary>
    public void Revoke()
    {
        Status = UserRoleStatuses.Revoked;
        RevokedAt = DateTimeOffset.UtcNow;
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
