using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSubscriptionLogCurrentAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresUtc",
                table: "MemberSubscriptionLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "MemberSubscriptionLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptionLog_ChapterId_MemberId",
                table: "MemberSubscriptionLog",
                columns: new[] { "ChapterId", "MemberId" },
                filter: "[IsCurrent] = 1");            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema-only reversal. The synthetic log rows from backfill B are left in place: dropping the
            // columns they carried makes them harmless, and re-applying Up is idempotent (its WHERE NOT
            // EXISTS skips members that already have a log row), so no duplicates on a down/up cycle.
            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptionLog_ChapterId_MemberId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "ExpiresUtc",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "MemberSubscriptionLog");
        }
    }
}
