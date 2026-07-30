using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSubscriptionsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberSubscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberSubscriptions",
                columns: table => new
                {
                    MemberChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderEmailSentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSubscriptions", x => x.MemberChapterId);
                    table.ForeignKey(
                        name: "FK_MemberSubscriptions_MemberChapters_MemberChapterId",
                        column: x => x.MemberChapterId,
                        principalTable: "MemberChapters",
                        principalColumn: "MemberChapterId",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
