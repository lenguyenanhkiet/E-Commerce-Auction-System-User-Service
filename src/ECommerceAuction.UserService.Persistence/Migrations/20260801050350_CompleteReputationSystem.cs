using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteReputationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[user].[LegacyReputationLedgerEntries]', N'U') IS NULL
                BEGIN
                    SELECT * INTO [user].[LegacyReputationLedgerEntries]
                    FROM [user].[ReputationLedgerEntries];
                END;

                DELETE FROM [user].[ReputationLedgerEntries]
                WHERE [status] <> 'CONFIRMED';
                """);

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_source_service_source_type_source_id",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_user_id_status_created_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReputationLedgerEntries_entry_type",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReputationLedgerEntries_points_non_zero",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReputationLedgerEntries_status",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "confirm_after",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "entry_type",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "reason",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "reversed_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "trust_level",
                schema: "user",
                table: "BuyerReputationProfiles");

            migrationBuilder.RenameColumn(
                name: "reversal_entry_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                newName: "reverses_entry_id");

            migrationBuilder.RenameColumn(
                name: "points",
                schema: "user",
                table: "ReputationLedgerEntries",
                newName: "score_delta");

            migrationBuilder.AlterColumn<string>(
                name: "source_type",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(80)",
                unicode: false,
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "source_service",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(80)",
                unicode: false,
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "source_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "rule_version",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "idempotency_key",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(450)",
                unicode: false,
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)");

            migrationBuilder.AddColumn<Guid>(
                name: "correlation_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "evidence_reference",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "message_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "occurred_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "reason_code",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "role",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "score_after",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "score_before",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "auction_restriction_status",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "NONE");

            migrationBuilder.AddColumn<string>(
                name: "blocking_violation_code",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_manual_review",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "restricted_until",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.Sql(
                """
                ;WITH OrderedLedger AS
                (
                    SELECT
                        [id],
                        COALESCE(
                            SUM([score_delta]) OVER (
                                PARTITION BY [user_id]
                                ORDER BY [created_at], [id]
                                ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING),
                            0) AS [calculated_before]
                    FROM [user].[ReputationLedgerEntries]
                )
                UPDATE target
                SET
                    [role] = 'BUYER',
                    [reason_code] = CONCAT(
                        'buyer.legacy.',
                        LOWER(REPLACE(REPLACE(archive.[reason], ' ', '-'), '_', '-'))),
                    [score_before] = ordered.[calculated_before],
                    [score_after] = ordered.[calculated_before] + target.[score_delta],
                    [message_id] = target.[id],
                    [occurred_at] = COALESCE(
                        archive.[confirmed_at],
                        archive.[created_at])
                FROM [user].[ReputationLedgerEntries] target
                INNER JOIN OrderedLedger ordered ON ordered.[id] = target.[id]
                INNER JOIN [user].[LegacyReputationLedgerEntries] archive
                    ON archive.[id] = target.[id];
                """);

            migrationBuilder.CreateTable(
                name: "SellerReputationProfiles",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    confirmed_score = table.Column<int>(type: "int", nullable: false),
                    selling_restriction_status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    auction_restriction_status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    restricted_until = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    requires_manual_review = table.Column<bool>(type: "bit", nullable: false),
                    blocking_violation_code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerReputationProfiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRatingEligibilities",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rater_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    opens_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: true),
                    submitted_rating_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRatingEligibilities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rater_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.id);
                    table.CheckConstraint("CK_UserRatings_score", "[score] >= 1 AND [score] <= 5");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_message_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_reverses_entry_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                column: "reverses_entry_id",
                unique: true,
                filter: "[reverses_entry_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReputationLedgerEntries_user_id_role_occurred_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                columns: new[] { "user_id", "role", "occurred_at" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReputationLedgerEntries_score_delta_non_zero",
                schema: "user",
                table: "ReputationLedgerEntries",
                sql: "[score_delta] <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_SellerReputationProfiles_user_id",
                schema: "user",
                table: "SellerReputationProfiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRatingEligibilities_transaction_type_transaction_id_rater_user_id_target_user_id",
                schema: "user",
                table: "TransactionRatingEligibilities",
                columns: new[] { "transaction_type", "transaction_id", "rater_user_id", "target_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_rater_user_id_created_at",
                schema: "user",
                table: "UserRatings",
                columns: new[] { "rater_user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_target_user_id_created_at",
                schema: "user",
                table: "UserRatings",
                columns: new[] { "target_user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_transaction_type_transaction_id_rater_user_id_target_user_id",
                schema: "user",
                table: "UserRatings",
                columns: new[] { "transaction_type", "transaction_id", "rater_user_id", "target_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerReputationProfiles",
                schema: "user");

            migrationBuilder.DropTable(
                name: "TransactionRatingEligibilities",
                schema: "user");

            migrationBuilder.DropTable(
                name: "UserRatings",
                schema: "user");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_message_id",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_reverses_entry_id",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReputationLedgerEntries_user_id_role_occurred_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReputationLedgerEntries_score_delta_non_zero",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "evidence_reference",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "message_id",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "occurred_at",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "reason_code",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "role",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "score_after",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "score_before",
                schema: "user",
                table: "ReputationLedgerEntries");

            migrationBuilder.DropColumn(
                name: "auction_restriction_status",
                schema: "user",
                table: "BuyerReputationProfiles");

            migrationBuilder.DropColumn(
                name: "blocking_violation_code",
                schema: "user",
                table: "BuyerReputationProfiles");

            migrationBuilder.DropColumn(
                name: "requires_manual_review",
                schema: "user",
                table: "BuyerReputationProfiles");

            migrationBuilder.DropColumn(
                name: "restricted_until",
                schema: "user",
                table: "BuyerReputationProfiles");

            migrationBuilder.RenameColumn(
                name: "score_delta",
                schema: "user",
                table: "ReputationLedgerEntries",
                newName: "points");

            migrationBuilder.RenameColumn(
                name: "reverses_entry_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                newName: "reversal_entry_id");

            migrationBuilder.AlterColumn<string>(
                name: "source_type",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(80)",
                oldUnicode: false,
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "source_service",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(80)",
                oldUnicode: false,
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "source_id",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldUnicode: false,
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "rule_version",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "idempotency_key",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldUnicode: false,
                oldMaxLength: 450);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirm_after",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirmed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "entry_type",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reason",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(60)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reversed_at",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "datetimeoffset(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "user",
                table: "ReputationLedgerEntries",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "trust_level",
                schema: "user",
                table: "BuyerReputationProfiles",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReputationLedgerEntries_entry_type",
                schema: "user",
                table: "ReputationLedgerEntries",
                sql: "[entry_type] IN ('PROFILE_VERIFICATION','ECOMMERCE_TRANSACTION','AUCTION_TRANSACTION','AUCTION_BONUS','REVIEW','PENALTY','REVERSAL')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReputationLedgerEntries_points_non_zero",
                schema: "user",
                table: "ReputationLedgerEntries",
                sql: "[points] <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReputationLedgerEntries_status",
                schema: "user",
                table: "ReputationLedgerEntries",
                sql: "[status] IN ('PENDING','CONFIRMED','REVERSED','CANCELLED')");
        }
    }
}
