using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyReputationProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReputationProfiles",
                schema: "user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReputationProfiles",
                schema: "user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    failed_auctions = table.Column<int>(type: "int", nullable: false),
                    failed_transactions = table.Column<int>(type: "int", nullable: false),
                    penalty_count = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    successful_auctions = table.Column<int>(type: "int", nullable: false),
                    successful_transactions = table.Column<int>(type: "int", nullable: false),
                    total_ratings = table.Column<int>(type: "int", nullable: false),
                    trust_level = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReputationProfiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_ReputationProfiles_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
