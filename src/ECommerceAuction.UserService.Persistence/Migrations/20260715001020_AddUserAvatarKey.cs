using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatarKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = 'user'
                      AND TABLE_NAME = 'Users'
                      AND COLUMN_NAME = 'avatar_key')
                BEGIN
                    ALTER TABLE [user].[Users] ADD [avatar_key] nvarchar(500) NULL;
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
                      AND COLUMN_NAME = 'avatar_key')
                BEGIN
                    ALTER TABLE [user].[Users] DROP COLUMN [avatar_key];
                END");
        }
    }
}
