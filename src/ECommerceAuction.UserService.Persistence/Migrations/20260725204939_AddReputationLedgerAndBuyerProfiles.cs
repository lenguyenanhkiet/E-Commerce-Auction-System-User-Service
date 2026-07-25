using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReputationLedgerAndBuyerProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuyerReputationProfiles",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    confirmed_score = table.Column<int>(type: "int", nullable: false),
                    pending_score = table.Column<int>(type: "int", nullable: false),
                    lifetime_earned_points = table.Column<long>(type: "bigint", nullable: false),
                    lifetime_penalty_points = table.Column<long>(type: "bigint", nullable: false),
                    successful_transactions = table.Column<int>(type: "int", nullable: false),
                    failed_transactions = table.Column<int>(type: "int", nullable: false),
                    successful_auctions = table.Column<int>(type: "int", nullable: false),
                    failed_auctions = table.Column<int>(type: "int", nullable: false),
                    penalty_count = table.Column<int>(type: "int", nullable: false),
                    trust_level = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerReputationProfiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BuyerVerificationProfiles",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_email_verified = table.Column<bool>(type: "bit", nullable: false),
                    is_phone_verified = table.Column<bool>(type: "bit", nullable: false),
                    is_identity_verified = table.Column<bool>(type: "bit", nullable: false),
                    has_verified_address = table.Column<bool>(type: "bit", nullable: false),
                    has_verified_payment_method = table.Column<bool>(type: "bit", nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    phone_verified_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    identity_verified_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    address_verified_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    payment_method_verified_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerVerificationProfiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ReputationLedgerEntries",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entry_type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(60)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    points = table.Column<int>(type: "int", nullable: false),
                    source_service = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    source_type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    source_id = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    idempotency_key = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    rule_version = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    reversal_entry_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    confirm_after = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    reversed_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReputationLedgerEntries", x => x.id);
                    table.CheckConstraint("CK_ReputationLedgerEntries_entry_type", "[entry_type] IN ('PROFILE_VERIFICATION','ECOMMERCE_TRANSACTION','AUCTION_TRANSACTION','AUCTION_BONUS','REVIEW','PENALTY','REVERSAL')");
                    table.CheckConstraint("CK_ReputationLedgerEntries_points_non_zero", "[points] <> 0");
                    table.CheckConstraint("CK_ReputationLedgerEntries_status", "[status] IN ('PENDING','CONFIRMED','REVERSED','CANCELLED')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuyerReputationProfiles_user_id",
                schema: "user",
                table: "BuyerReputationProfiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuyerVerificationProfiles_user_id",
                schema: "user",
                table: "BuyerVerificationProfiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_idempotency_key",
                schema: "user",
                table: "ReputationLedgerEntries",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_source_service_source_type_source_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                columns: new[] { "source_service", "source_type", "source_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                columns: new[] { "user_id", "status", "created_at" });

            // ---------------------------------------------------------------------------------
            // Backfill (idempotent). Every statement is guarded by NOT EXISTS so re-running the
            // migration never awards a point twice and never inflates a summary score.
            // ---------------------------------------------------------------------------------
            migrationBuilder.Sql(
                """
                DECLARE @now datetime2(3) = SYSUTCDATETIME();

                -- 1. A buyer verification profile (email verified) for every already-email-confirmed user.
                INSERT INTO [user].[BuyerVerificationProfiles]
                    (id, user_id, is_email_verified, is_phone_verified, is_identity_verified,
                     has_verified_address, has_verified_payment_method,
                     email_verified_at, created_at, updated_at)
                SELECT NEWID(), u.[id], 1, 0, 0, 0, 0, u.[created_at], u.[created_at], @now
                FROM [user].[Users] u
                WHERE u.[email_verified] = 1
                  AND u.[deleted_at] IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM [user].[BuyerVerificationProfiles] b WHERE b.[user_id] = u.[id]);

                -- 2. One confirmed EMAIL_VERIFIED ledger entry per such user (history only). The
                --    idempotency key matches the application: user:{userId-lowercase}:email-verified:v1.
                INSERT INTO [user].[ReputationLedgerEntries]
                    (id, user_id, entry_type, reason, status, points,
                     source_service, source_type, source_id,
                     idempotency_key, rule_version, confirmed_at, created_at, updated_at)
                SELECT NEWID(), u.[id], 'PROFILE_VERIFICATION', 'EMAIL_VERIFIED', 'CONFIRMED', 1,
                       'user-service', 'USER_EMAIL', LOWER(CONVERT(nvarchar(36), u.[id])),
                       'user:' + LOWER(CONVERT(nvarchar(36), u.[id])) + ':email-verified:v1',
                       'REPUTATION_V1', u.[created_at], u.[created_at], @now
                FROM [user].[Users] u
                WHERE u.[email_verified] = 1
                  AND u.[deleted_at] IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM [user].[ReputationLedgerEntries] l
                      WHERE l.[idempotency_key] =
                            'user:' + LOWER(CONVERT(nvarchar(36), u.[id])) + ':email-verified:v1');

                -- 3. Seed the new buyer reputation summary from the legacy score (do NOT re-add the
                --    email point: the legacy score already includes it).
                INSERT INTO [user].[BuyerReputationProfiles]
                    (id, user_id, confirmed_score, pending_score,
                     lifetime_earned_points, lifetime_penalty_points,
                     successful_transactions, failed_transactions,
                     successful_auctions, failed_auctions, penalty_count,
                     trust_level, created_at, updated_at)
                SELECT NEWID(), r.[user_id], r.[score], 0,
                       CASE WHEN r.[score] > 0 THEN r.[score] ELSE 0 END, 0,
                       r.[successful_transactions], r.[failed_transactions],
                       r.[successful_auctions], r.[failed_auctions], r.[penalty_count],
                       CASE
                           WHEN r.[score] < 0    THEN 'RESTRICTED'
                           WHEN r.[score] < 50   THEN 'BASIC'
                           WHEN r.[score] < 200  THEN 'TRUSTED'
                           WHEN r.[score] < 500  THEN 'RELIABLE'
                           WHEN r.[score] < 1000 THEN 'PREMIUM'
                           ELSE 'ELITE'
                       END,
                       @now, @now
                FROM [user].[ReputationProfiles] r
                WHERE NOT EXISTS (
                    SELECT 1 FROM [user].[BuyerReputationProfiles] b WHERE b.[user_id] = r.[user_id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyerReputationProfiles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "BuyerVerificationProfiles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "ReputationLedgerEntries",
                schema: "user");
        }
    }
}
