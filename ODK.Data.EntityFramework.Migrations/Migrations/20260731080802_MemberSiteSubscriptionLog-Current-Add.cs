using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionLogCurrentAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Payments FK was created manually as FK_MemberSiteSubscriptionLog_Payments (not the EF
            // convention name FK_..._Payments_PaymentId), so drop it by looking it up rather than by a
            // hard-coded name. Altering PaymentId to nullable needs the FK dropped first; EF re-adds it
            // (convention-named) below, reconciling the name across environments.
            migrationBuilder.Sql(@"
DECLARE @fk sysname;
SELECT @fk = fk.name
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID(N'MemberSiteSubscriptionLog')
  AND fk.referenced_object_id = OBJECT_ID(N'Payments');
IF @fk IS NOT NULL
    EXEC('ALTER TABLE [MemberSiteSubscriptionLog] DROP CONSTRAINT [' + @fk + ']');");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "MemberSiteSubscriptionLog",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledUtc",
                table: "MemberSiteSubscriptionLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresUtc",
                table: "MemberSiteSubscriptionLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "MemberSiteSubscriptionLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "MemberSiteSubscriptionLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "MemberSiteSubscriptionLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptionLog_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptionLog_MemberId_Current",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                filter: "[IsCurrent] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropIndex(
                name: "IX_MemberSiteSubscriptionLog_MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropIndex(
                name: "IX_MemberSiteSubscriptionLog_MemberId_Current",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "CancelledUtc",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "ExpiresUtc",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "MemberSiteSubscriptionLog",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
