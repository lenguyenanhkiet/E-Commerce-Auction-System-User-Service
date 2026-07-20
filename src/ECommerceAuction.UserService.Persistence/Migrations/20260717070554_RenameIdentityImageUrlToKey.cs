using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdentityImageUrlToKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentityFrontImageUrl",
                schema: "user",
                table: "IdentityVerification",
                newName: "IdentityFrontImageKey");

            migrationBuilder.RenameColumn(
                name: "IdentityBackImageUrl",
                schema: "user",
                table: "IdentityVerification",
                newName: "IdentityBackImageKey");

            // Renaming keeps the old values, which are public URLs ("{baseUrl}/{key}"), while the
            // code now feeds this column to IStorageProvider.DownloadAsync as a storage key. Both
            // storage providers build their URL as baseUrl + "/" + key and every identity key starts
            // with "user/identity/", so cutting from that marker recovers the key whichever baseUrl
            // was in force. Rows already holding a bare key start at position 1 and are left as-is,
            // which also makes this safe to re-run.
            migrationBuilder.Sql("""
                UPDATE [user].[IdentityVerification]
                SET IdentityFrontImageKey =
                        SUBSTRING(IdentityFrontImageKey,
                                  CHARINDEX('user/identity/', IdentityFrontImageKey),
                                  LEN(IdentityFrontImageKey)),
                    IdentityBackImageKey =
                        SUBSTRING(IdentityBackImageKey,
                                  CHARINDEX('user/identity/', IdentityBackImageKey),
                                  LEN(IdentityBackImageKey))
                WHERE CHARINDEX('user/identity/', IdentityFrontImageKey) > 0
                   OR CHARINDEX('user/identity/', IdentityBackImageKey) > 0;
                """);
        }

        /// <summary>
        /// Only the column names are restored, not their old contents: rebuilding a public URL needs
        /// the storage base address that was configured when each row was written, which is not
        /// recorded anywhere. After a rollback these columns hold storage keys under a name that
        /// says Url.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentityFrontImageKey",
                schema: "user",
                table: "IdentityVerification",
                newName: "IdentityFrontImageUrl");

            migrationBuilder.RenameColumn(
                name: "IdentityBackImageKey",
                schema: "user",
                table: "IdentityVerification",
                newName: "IdentityBackImageUrl");
        }
    }
}
