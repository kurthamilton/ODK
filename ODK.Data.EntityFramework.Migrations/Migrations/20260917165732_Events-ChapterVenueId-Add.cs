using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EventsChapterVenueIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChapterVenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events",
                column: "ChapterVenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ChapterVenues_ChapterVenueId",
                table: "Events",
                column: "ChapterVenueId",
                principalTable: "ChapterVenues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_ChapterVenues_ChapterVenueId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ChapterVenueId",
                table: "Events");
        }
    }
}
