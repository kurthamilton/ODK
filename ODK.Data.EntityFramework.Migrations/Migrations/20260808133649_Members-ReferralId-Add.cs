using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MembersReferralIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferralId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ReferralId",
                table: "Members",
                column: "ReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Referrals_ReferralId",
                table: "Members",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "ReferralId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Referrals_ReferralId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_ReferralId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "Members");
        }
    }
}
