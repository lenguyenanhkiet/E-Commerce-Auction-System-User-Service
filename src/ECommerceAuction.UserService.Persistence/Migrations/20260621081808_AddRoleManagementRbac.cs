using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleManagementRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "user",
                table: "Roles",
                type: "nvarchar(150)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.CreateTable(
                name: "Privileges",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RolePrivileges",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    privilege_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePrivileges", x => x.id);
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Privileges_privilege_id",
                        column: x => x.privilege_id,
                        principalSchema: "user",
                        principalTable: "Privileges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePrivileges_Roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "user",
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_code",
                schema: "user",
                table: "Privileges",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivileges_privilege_id",
                schema: "user",
                table: "RolePrivileges",
                column: "privilege_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivileges_role_id_privilege_id",
                schema: "user",
                table: "RolePrivileges",
                columns: new[] { "role_id", "privilege_id" },
                unique: true);

            // Idempotent seed keeps existing OAuth-created BUYER rows compatible.
            migrationBuilder.Sql(
                """
                DECLARE @now datetime2(3) = SYSUTCDATETIME();

                IF NOT EXISTS (SELECT 1 FROM [user].[Roles] WHERE [code] = 'ADMIN')
                    INSERT INTO [user].[Roles] ([id], [code], [name], [description], [is_system_role], [status], [created_at], [updated_at])
                    VALUES ('11111111-1111-1111-1111-111111111111', 'ADMIN', 'Administrator', 'System administrator role.', 1, 'ACTIVE', @now, @now);

                IF NOT EXISTS (SELECT 1 FROM [user].[Roles] WHERE [code] = 'BUYER')
                    INSERT INTO [user].[Roles] ([id], [code], [name], [description], [is_system_role], [status], [created_at], [updated_at])
                    VALUES ('22222222-2222-2222-2222-222222222222', 'BUYER', 'Buyer', 'Default role for users who buy products and join auctions.', 1, 'ACTIVE', @now, @now);

                IF NOT EXISTS (SELECT 1 FROM [user].[Roles] WHERE [code] = 'SELLER')
                    INSERT INTO [user].[Roles] ([id], [code], [name], [description], [is_system_role], [status], [created_at], [updated_at])
                    VALUES ('33333333-3333-3333-3333-333333333333', 'SELLER', 'Seller', 'System role for sellers.', 1, 'ACTIVE', @now, @now);

                IF NOT EXISTS (SELECT 1 FROM [user].[Roles] WHERE [code] = 'SUPPORT_STAFF')
                    INSERT INTO [user].[Roles] ([id], [code], [name], [description], [is_system_role], [status], [created_at], [updated_at])
                    VALUES ('44444444-4444-4444-4444-444444444444', 'SUPPORT_STAFF', 'Support Staff', 'System role for support staff.', 1, 'ACTIVE', @now, @now);

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'ROLE.CREATE')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000001', 'ROLE.CREATE', 'Create Role', 'Create custom RBAC roles.', 'ACTIVE');

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'ROLE.UPDATE')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000002', 'ROLE.UPDATE', 'Update Role', 'Update custom RBAC roles.', 'ACTIVE');

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'ROLE.DELETE')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000003', 'ROLE.DELETE', 'Delete Role', 'Soft-delete unused custom RBAC roles.', 'ACTIVE');

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'ROLE.VIEW')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000004', 'ROLE.VIEW', 'View Role', 'View one role and its privileges.', 'ACTIVE');

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'ROLE.LIST')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000005', 'ROLE.LIST', 'List Roles', 'Search and list RBAC roles.', 'ACTIVE');

                IF NOT EXISTS (SELECT 1 FROM [user].[Privileges] WHERE [code] = 'PRIVILEGE.LIST')
                    INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                    VALUES ('aaaaaaaa-0001-0001-0001-000000000006', 'PRIVILEGE.LIST', 'List Privileges', 'List active privileges for role assignment.', 'ACTIVE');

                DECLARE @adminRoleId uniqueidentifier = (SELECT TOP (1) [id] FROM [user].[Roles] WHERE [code] = 'ADMIN');

                INSERT INTO [user].[RolePrivileges] ([id], [role_id], [privilege_id], [assigned_by], [assigned_at])
                SELECT NEWID(), @adminRoleId, p.[id], NULL, @now
                FROM [user].[Privileges] p
                WHERE p.[code] IN ('ROLE.CREATE', 'ROLE.UPDATE', 'ROLE.DELETE', 'ROLE.VIEW', 'ROLE.LIST', 'PRIVILEGE.LIST')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [user].[RolePrivileges] rp
                      WHERE rp.[role_id] = @adminRoleId
                        AND rp.[privilege_id] = p.[id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePrivileges",
                schema: "user");

            migrationBuilder.DropTable(
                name: "Privileges",
                schema: "user");

            migrationBuilder.Sql(
                """
                DELETE r
                FROM [user].[Roles] r
                WHERE r.[id] IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444')
                  AND NOT EXISTS (
                      SELECT 1 FROM [user].[UserRoles] ur WHERE ur.[role_id] = r.[id]);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "user",
                table: "Roles",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)");
        }
    }
}
