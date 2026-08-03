using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionLogMemberIdCascadeDeleteFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }
    }
}
