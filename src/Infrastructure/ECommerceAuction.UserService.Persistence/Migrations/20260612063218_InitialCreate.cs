using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "user",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReputationProfiles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "user");
        }
    }
}
