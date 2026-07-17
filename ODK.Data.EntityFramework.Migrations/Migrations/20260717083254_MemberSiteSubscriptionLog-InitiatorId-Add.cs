using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionLogInitiatorIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InitiatorId",
                table: "MemberSiteSubscriptionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptionLog_InitiatorId",
                table: "MemberSiteSubscriptionLog",
                column: "InitiatorId",
                unique: true,
                filter: "[InitiatorId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberSiteSubscriptionLog_InitiatorId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "InitiatorId",
                table: "MemberSiteSubscriptionLog");
        }
    }
}
