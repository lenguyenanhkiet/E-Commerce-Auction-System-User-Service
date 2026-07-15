using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthProviderToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auth_provider",
                schema: "user",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "LOCAL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auth_provider",
                schema: "user",
                table: "Users");
        }
    }
}
