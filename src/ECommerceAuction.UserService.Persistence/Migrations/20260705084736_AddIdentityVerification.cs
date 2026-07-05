using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_identity_number",
                schema: "user",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "identity_number",
                schema: "user",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "SellerProfiles",
                newName: "SellerProfiles",
                newSchema: "user");

            migrationBuilder.CreateTable(
                name: "IdentityVerification",
                schema: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentityFrontImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IdentityBackImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityVerification", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerification_UserId",
                schema: "user",
                table: "IdentityVerification",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityVerification",
                schema: "user");

            migrationBuilder.RenameTable(
                name: "SellerProfiles",
                schema: "user",
                newName: "SellerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "identity_number",
                schema: "user",
                table: "Users",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_identity_number",
                schema: "user",
                table: "Users",
                column: "identity_number",
                unique: true,
                filter: "[identity_number] IS NOT NULL");
        }
    }
}
