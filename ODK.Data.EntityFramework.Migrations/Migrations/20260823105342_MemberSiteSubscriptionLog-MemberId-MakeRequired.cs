using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionLogMemberIdMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server will not modify a column a foreign key constraint is defined on, so the key comes
            // off for the alter and goes back on after it. The indexes on the column need no such handling -
            // EF drops and recreates those around a narrowing alter itself.
            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "MemberId");

            /* No defaultValue, which the scaffolder supplies as an empty guid: MemberSiteSubscriptionLog-Backfill
               populated every row that predates the column, so there is no null for one to fill, and an empty
               guid is not a member - it would only fail the foreign key added back below. A row without a
               member must fail the alter rather than be given a value nothing means. */
            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "MemberSiteSubscriptionLog",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "MemberId");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "MemberSiteSubscriptionLog",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
