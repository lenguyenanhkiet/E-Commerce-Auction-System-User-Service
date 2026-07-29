using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestoreFullBuyerVerificationAndAddBankVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "identity_verified_at",
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
                name: "is_identity_verified",
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

            migrationBuilder.Sql(
                """
                INSERT INTO [user].[BuyerVerificationProfiles]
                    ([id], [user_id], [is_email_verified], [is_phone_verified],
                     [is_identity_verified], [has_verified_address],
                     [has_verified_payment_method], [email_verified_at],
                     [phone_verified_at], [identity_verified_at],
                     [address_verified_at], [payment_method_verified_at],
                     [created_at], [updated_at], [deleted_at])
                SELECT
                    NEWID(), u.[id], u.[email_verified], u.[phone_verified],
                    u.[identity_verified], 0, 0,
                    CASE WHEN u.[email_verified] = 1 THEN u.[updated_at] END,
                    CASE WHEN u.[phone_verified] = 1 THEN u.[updated_at] END,
                    CASE WHEN u.[identity_verified] = 1 THEN u.[updated_at] END,
                    NULL, NULL, u.[created_at], u.[updated_at], NULL
                FROM [user].[Users] u
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [user].[BuyerVerificationProfiles] b
                    WHERE b.[user_id] = u.[id]);

                UPDATE b
                SET
                    b.[is_email_verified] = u.[email_verified],
                    b.[is_phone_verified] = u.[phone_verified],
                    b.[is_identity_verified] = u.[identity_verified],
                    b.[email_verified_at] =
                        CASE WHEN u.[email_verified] = 1 THEN u.[updated_at] END,
                    b.[phone_verified_at] =
                        CASE WHEN u.[phone_verified] = 1 THEN u.[updated_at] END,
                    b.[identity_verified_at] =
                        CASE WHEN u.[identity_verified] = 1 THEN u.[updated_at] END
                FROM [user].[BuyerVerificationProfiles] b
                INNER JOIN [user].[Users] u ON u.[id] = b.[user_id];
                """);

            migrationBuilder.CreateTable(
                name: "BankAccountVerifications",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    provider_reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    bank_code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    masked_account_number = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    account_fingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expected_account_holder_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    verified_account_holder_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    failure_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    failure_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountVerifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountVerifications_account_fingerprint",
                schema: "user",
                table: "BankAccountVerifications",
                column: "account_fingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountVerifications_provider_provider_reference",
                schema: "user",
                table: "BankAccountVerifications",
                columns: new[] { "provider", "provider_reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountVerifications_user_id",
                schema: "user",
                table: "BankAccountVerifications",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccountVerifications",
                schema: "user");

            migrationBuilder.DropColumn(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "identity_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "is_email_verified",
                schema: "user",
                table: "BuyerVerificationProfiles");

            migrationBuilder.DropColumn(
                name: "is_identity_verified",
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
    }
}
