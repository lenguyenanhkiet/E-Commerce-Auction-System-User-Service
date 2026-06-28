using System.Text.Json;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Roles.Common;

/// <summary>
/// Produces consistent old/new JSON snapshots for Role Management audit logs.
/// </summary>
public static class RoleAuditSerializer
{
    public static string Serialize(Role role, IEnumerable<string> privilegeCodes)
    {
        return JsonSerializer.Serialize(new
        {
            role.Code,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.Status,
            PrivilegeCodes = privilegeCodes.OrderBy(code => code).ToArray()
        });
    }
}
