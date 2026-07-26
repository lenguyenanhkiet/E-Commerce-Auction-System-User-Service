using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateIdentityVerificationFromBuyerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "is_identity_verified",
                schema: "user",
                table: "BuyerVerificationProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "identity_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_identity_verified",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
