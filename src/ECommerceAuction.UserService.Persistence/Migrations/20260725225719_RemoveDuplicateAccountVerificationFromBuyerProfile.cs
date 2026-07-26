using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateAccountVerificationFromBuyerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "is_email_verified",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "is_phone_verified",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "phone_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_email_verified",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_phone_verified",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "phone_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true);
        }
    }
}
