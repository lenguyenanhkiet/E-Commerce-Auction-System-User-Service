namespace ECommerceAuction.UserService.Persistence.Seed;

/// <summary>
/// Stable identifiers used by the Role Management RBAC migration and tests.
/// Existing rows are matched by code so older databases remain compatible.
/// </summary>
public static class RbacSeedData
{
    public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BuyerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SellerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid SupportStaffRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static readonly Guid RoleCreatePrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid RoleUpdatePrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid RoleDeletePrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid RoleViewPrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid RoleListPrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    // Kept only to identify the legacy PRIVILEGE.LIST row created by the first RBAC migration.
    public static readonly Guid LegacyPrivilegeListPrivilegeId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
}
