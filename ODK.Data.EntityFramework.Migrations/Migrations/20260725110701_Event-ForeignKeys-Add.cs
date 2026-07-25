using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EventForeignKeysAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*remove indexes manually created but not included in initial migration*/
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Events",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Members",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_EventId",
                table: "EventResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_MemberId",
                table: "EventResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventResponses_MemberId",
                table: "EventResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventInvites_MemberId",
                table: "EventInvites");

            /*auto-generated FKs and indexes*/
            migrationBuilder.CreateIndex(
                name: "IX_EventResponses_MemberId",
                table: "EventResponses",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_EventInvites_MemberId",
                table: "EventInvites",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Members_MemberId",
                table: "EventInvites",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Events_EventId",
                table: "EventResponses",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Members_MemberId",
                table: "EventResponses",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Members_MemberId",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_Events_EventId",
                table: "EventResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_Members_MemberId",
                table: "EventResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventResponses_MemberId",
                table: "EventResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventInvites_MemberId",
                table: "EventInvites");
        }
    }
}