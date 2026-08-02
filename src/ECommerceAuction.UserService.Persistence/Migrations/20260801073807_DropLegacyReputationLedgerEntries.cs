using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyReputationLedgerEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[user].[LegacyReputationLedgerEntries]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [user].[LegacyReputationLedgerEntries];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[user].[LegacyReputationLedgerEntries]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [user].[LegacyReputationLedgerEntries]
                    (
                        [id] uniqueidentifier NOT NULL,
                        [user_id] uniqueidentifier NOT NULL,
                        [entry_type] nvarchar(50) NOT NULL,
                        [reason] nvarchar(60) NOT NULL,
                        [status] nvarchar(20) NOT NULL,
                        [points] int NOT NULL,
                        [source_service] nvarchar(50) NOT NULL,
                        [source_type] nvarchar(50) NOT NULL,
                        [source_id] nvarchar(200) NOT NULL,
                        [idempotency_key] nvarchar(200) NOT NULL,
                        [rule_version] nvarchar(50) NOT NULL,
                        [reversal_entry_id] uniqueidentifier NULL,
                        [confirm_after] datetimeoffset(3) NULL,
                        [confirmed_at] datetimeoffset(3) NULL,
                        [cancelled_at] datetimeoffset(3) NULL,
                        [reversed_at] datetimeoffset(3) NULL,
                        [created_at] datetimeoffset(3) NOT NULL,
                        [updated_at] datetimeoffset(3) NULL,
                        [deleted_at] datetimeoffset(3) NULL
                    );
                END
                """);
        }
    }
}
