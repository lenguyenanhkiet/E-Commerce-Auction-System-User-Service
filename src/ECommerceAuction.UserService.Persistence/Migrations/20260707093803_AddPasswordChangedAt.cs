using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "password_changed_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: true);

            // Seed existing local-password accounts so their password age starts from creation
            // instead of being treated as "never set" by the password-expiry job.
            migrationBuilder.Sql(
                "UPDATE [user].[Users] SET [password_changed_at] = [created_at] " +
                "WHERE [password_changed_at] IS NULL AND [password_hash] <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_changed_at",
                schema: "user",
                table: "Users");
        }
    }
}
