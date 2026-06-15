using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserServiceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    is_system_role = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserAuditLogs",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    target_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    entity_id = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    identity_number = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    password_changed_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false, defaultValue: "ACTIVE"),
                    email_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    phone_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    failed_login_attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    locked_until = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    device_id = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    device_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    last_used_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ReputationProfiles",
                schema: "user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    trust_level = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    total_ratings = table.Column<int>(type: "int", nullable: false),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    successful_transactions = table.Column<int>(type: "int", nullable: false),
                    failed_transactions = table.Column<int>(type: "int", nullable: false),
                    successful_auctions = table.Column<int>(type: "int", nullable: false),
                    failed_auctions = table.Column<int>(type: "int", nullable: false),
                    penalty_count = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReputationProfiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_ReputationProfiles_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserExternalLogins",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    provider_user_id = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    provider_email = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    provider_display_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    access_token_hash = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    refresh_token_hash = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    linked_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExternalLogins", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserExternalLogins_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "user",
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_code",
                schema: "user",
                table: "Roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_action",
                schema: "user",
                table: "UserAuditLogs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_target_user_id",
                schema: "user",
                table: "UserAuditLogs",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_provider_provider_user_id",
                schema: "user",
                table: "UserExternalLogins",
                columns: new[] { "provider", "provider_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_user_id",
                schema: "user",
                table: "UserExternalLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_role_id",
                schema: "user",
                table: "UserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_user_id_role_id",
                schema: "user",
                table: "UserRoles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "[revoked_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                schema: "user",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_identity_number",
                schema: "user",
                table: "Users",
                column: "identity_number",
                unique: true,
                filter: "[identity_number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_phone_number",
                schema: "user",
                table: "Users",
                column: "phone_number",
                unique: true,
                filter: "[phone_number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_refresh_token_hash",
                schema: "user",
                table: "UserSessions",
                column: "refresh_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReputationProfiles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserAuditLogs",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserExternalLogins",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "user");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "user");
        }
    }
}
