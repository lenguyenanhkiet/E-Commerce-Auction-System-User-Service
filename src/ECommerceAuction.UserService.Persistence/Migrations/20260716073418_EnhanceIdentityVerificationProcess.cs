using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceIdentityVerificationProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "identity_verified",
                schema: "user",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "user",
                table: "IdentityVerification",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                schema: "user",
                table: "IdentityVerification",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "user",
                table: "IdentityVerification",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "user",
                table: "IdentityVerification",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "IssueDate",
                schema: "user",
                table: "IdentityVerification",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "IssuePlace",
                schema: "user",
                table: "IdentityVerification",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddress",
                schema: "user",
                table: "IdentityVerification",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_verified",
                schema: "user",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "IssuePlace",
                schema: "user",
                table: "IdentityVerification");

            migrationBuilder.DropColumn(
                name: "PermanentAddress",
                schema: "user",
                table: "IdentityVerification");
        }
    }
}
