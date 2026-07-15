using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent guard: this staging DB has a history of schema drift (column present but
            // migration not recorded), which would otherwise fail re-runs with SQL error 2705.
            migrationBuilder.Sql(
                @"IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'user'
                      AND TABLE_NAME = 'Users'
                      AND COLUMN_NAME = 'avatar_url')
                BEGIN
                    ALTER TABLE [user].[Users] ADD [avatar_url] nvarchar(500) NULL;
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'user'
                      AND TABLE_NAME = 'Users'
                      AND COLUMN_NAME = 'avatar_url')
                BEGIN
                    ALTER TABLE [user].[Users] DROP COLUMN [avatar_url];
                END");
        }
    }
}
