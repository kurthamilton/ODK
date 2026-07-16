using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberSubscriptionRecordInitiatorIdUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InitiatorId",
                table: "MemberSubscriptionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptionLog_InitiatorId",
                table: "MemberSubscriptionLog",
                column: "InitiatorId",
                unique: true,
                filter: "[InitiatorId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptionLog_InitiatorId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "InitiatorId",
                table: "MemberSubscriptionLog");
        }
    }
}
