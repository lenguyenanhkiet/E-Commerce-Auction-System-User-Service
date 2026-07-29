namespace ECommerceAuction.UserService.Domain.Roles;

/// <summary>
/// Represents the current assignment of one privilege to one role.
/// Assignment history is stored in UserAuditLogs.
/// </summary>
public sealed class RolePrivilege
{
    private RolePrivilege()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RoleId { get; private set; }
    public Guid PrivilegeId { get; private set; }
    public Guid? AssignedBy { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Role Role { get; private set; } = null!;
    public Privilege Privilege { get; private set; } = null!;

    /// <summary>
    /// Creates a role-to-privilege assignment for the current RBAC permission matrix.
    /// </summary>
    public static RolePrivilege Assign(Guid roleId, Guid privilegeId, Guid? assignedBy)
    {
        return new RolePrivilege
        {
            RoleId = roleId,
            PrivilegeId = privilegeId,
            AssignedBy = assignedBy,
            AssignedAt = DateTimeOffset.UtcNow
        };
    }
}