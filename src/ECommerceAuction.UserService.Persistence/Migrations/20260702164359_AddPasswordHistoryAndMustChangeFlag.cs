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
               migrationBuilder.AddColumn<bool>(
               name: "must_change_password",
               schema: "user",
               table: "Users",
               type: "bit",
               nullable: false,
               defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
               migrationBuilder.DropColumn(
               name: "must_change_password",
               schema: "user",
               table: "Users");
        }
    }
}
