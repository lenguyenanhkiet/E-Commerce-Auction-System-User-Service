using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveIdentityVerificationOutOfSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityCardBackUrl",
                schema: "user",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "IdentityCardFrontUrl",
                schema: "user",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                schema: "user",
                table: "SellerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityCardBackUrl",
                schema: "user",
                table: "SellerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityCardFrontUrl",
                schema: "user",
                table: "SellerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                schema: "user",
                table: "SellerProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
