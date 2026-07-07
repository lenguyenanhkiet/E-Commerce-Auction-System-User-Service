using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <summary>
    /// Drops the orphaned PascalCase columns left over from the pre-snake_case mapping.
    /// EF maps MustChangePassword/StatusExpiresAt to must_change_password/status_expires_at,
    /// so these duplicate columns are dead. Guarded with COL_LENGTH so the migration is safe
    /// on environments where the orphans may already be gone.
    /// </summary>
    public partial class DropOrphanUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropOrphanColumn("MustChangePassword"));
            migrationBuilder.Sql(DropOrphanColumn("StatusExpiresAt"));
        }

        /// <summary>
        /// Drops any auto-named default constraint on the column first, then the column itself.
        /// </summary>
        private static string DropOrphanColumn(string columnName) =>
            $@"
DECLARE @constraint sysname;
SELECT @constraint = dc.name
FROM sys.default_constraints dc
JOIN sys.columns col ON col.default_object_id = dc.object_id
WHERE col.object_id = OBJECT_ID('[user].[Users]') AND col.name = '{columnName}';
IF @constraint IS NOT NULL
    EXEC('ALTER TABLE [user].[Users] DROP CONSTRAINT [' + @constraint + ']');
IF COL_LENGTH('[user].[Users]', '{columnName}') IS NOT NULL
    ALTER TABLE [user].[Users] DROP COLUMN [{columnName}];";

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('[user].[Users]', 'MustChangePassword') IS NULL " +
                "ALTER TABLE [user].[Users] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);");

            migrationBuilder.Sql(
                "IF COL_LENGTH('[user].[Users]', 'StatusExpiresAt') IS NULL " +
                "ALTER TABLE [user].[Users] ADD [StatusExpiresAt] datetime2 NULL;");
        }
    }
}
