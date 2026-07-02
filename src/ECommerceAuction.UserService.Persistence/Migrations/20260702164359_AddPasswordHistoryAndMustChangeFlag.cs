using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHistoryAndMustChangeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MustChangePassword",
                schema: "user",
                table: "Users",
                newName: "must_change_password");

            migrationBuilder.AlterColumn<bool>(
                name: "must_change_password",
                schema: "user",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "must_change_password",
                schema: "user",
                table: "Users",
                newName: "MustChangePassword");

            migrationBuilder.AlterColumn<bool>(
                name: "MustChangePassword",
                schema: "user",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
