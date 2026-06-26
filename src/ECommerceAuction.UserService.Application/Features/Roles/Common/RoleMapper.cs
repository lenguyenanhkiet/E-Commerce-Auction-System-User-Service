using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Roles.Common;

/// <summary>
/// Maps domain entities to stable Role Management API contracts.
/// </summary>
public static class RoleMapper
{
    public static RoleResponse ToResponse(Role role)
    {
        return ToResponse(
            role,
            role.RolePrivileges.Select(rolePrivilege => rolePrivilege.Privilege));
    }

    public static RoleResponse ToResponse(Role role, IEnumerable<Privilege> privileges)
    {
        return new RoleResponse(
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.Status,
            privileges
                .OrderBy(privilege => privilege.Code)
                .Select(privilege => new PrivilegeResponse(
                    privilege.Id,
                    privilege.Code,
                    privilege.Name,
                    privilege.Description))
                .ToArray());
    }
}
