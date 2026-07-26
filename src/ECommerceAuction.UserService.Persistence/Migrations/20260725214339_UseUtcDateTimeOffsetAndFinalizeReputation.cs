using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseUtcDateTimeOffsetAndFinalizeReputation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId_IsDefault",
                schema: "user",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_PasswordHistories_UserId_CreatedAt",
                schema: "user",
                table: "PasswordHistories");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_user_id_role_id",
                schema: "user",
                table: "UserRoles");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "revoked_at",
                schema: "user",
                table: "UserSessions",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_used_at",
                schema: "user",
                table: "UserSessions",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expires_at",
                schema: "user",
                table: "UserSessions",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "UserSessions",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "status_expires_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "password_changed_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_login_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "Users",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "revoked_at",
                schema: "user",
                table: "UserRoles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "assigned_at",
                schema: "user",
                table: "UserRoles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "linked_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_login_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "UserAuditLogs",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SubmittedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SellerApplicationHistories",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "SellerApplicationHistories",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SellerApplicationHistories",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ChangedAt",
                table: "SellerApplicationHistories",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "Roles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "Roles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "Roles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "assigned_at",
                schema: "user",
                table: "RolePrivileges",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "ReputationProfiles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "reversed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "confirmed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "confirm_after",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "cancelled_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expiry_date",
                schema: "user",
                table: "PasswordResetTokens",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "PasswordResetTokens",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "user",
                table: "PasswordHistories",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VerifiedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SubmittedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "phone_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "payment_method_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "identity_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "address_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "user",
                table: "Addresses",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "user",
                table: "Addresses",
                type: "datetimeoffset(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "user",
                table: "Addresses",
                type: "datetimeoffset(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId_IsDefault",
                schema: "user",
                table: "Addresses",
                columns: new[] { "UserId", "IsDefault" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId_CreatedAt",
                schema: "user",
                table: "PasswordHistories",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                columns: new[] { "user_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_user_id_role_id",
                schema: "user",
                table: "UserRoles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "[revoked_at] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId_IsDefault",
                schema: "user",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_PasswordHistories_UserId_CreatedAt",
                schema: "user",
                table: "PasswordHistories");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_user_id_role_id",
                schema: "user",
                table: "UserRoles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "revoked_at",
                schema: "user",
                table: "UserSessions",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_used_at",
                schema: "user",
                table: "UserSessions",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                schema: "user",
                table: "UserSessions",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "UserSessions",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "status_expires_at",
                schema: "user",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "password_changed_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_login_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "Users",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "revoked_at",
                schema: "user",
                table: "UserRoles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "assigned_at",
                schema: "user",
                table: "UserRoles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "linked_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_login_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "UserExternalLogins",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "UserAuditLogs",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReviewedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "user",
                table: "SellerProfiles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SellerApplicationHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "SellerApplicationHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SellerApplicationHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChangedAt",
                table: "SellerApplicationHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "Roles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "Roles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "Roles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "assigned_at",
                schema: "user",
                table: "RolePrivileges",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "ReputationProfiles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "reversed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "confirmed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "confirm_after",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "cancelled_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiry_date",
                schema: "user",
                table: "PasswordResetTokens",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "PasswordResetTokens",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "user",
                table: "PasswordHistories",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VerifiedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "user",
                table: "IdentityVerification",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "phone_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "payment_method_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "identity_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "email_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "address_verified_at",
                schema: "user",
                table: "BuyerVerificationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "user",
                table: "Addresses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "user",
                table: "Addresses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "user",
                table: "Addresses",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(3)");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId_IsDefault",
                schema: "user",
                table: "Addresses",
                columns: new[] { "UserId", "IsDefault" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId_CreatedAt",
                schema: "user",
                table: "PasswordHistories",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                columns: new[] { "user_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_user_id_role_id",
                schema: "user",
                table: "UserRoles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "[revoked_at] IS NULL");
        }
    }
}
