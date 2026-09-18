using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterVenuesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterVenues",
                table: "ChapterVenues");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterVenues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterVenues",
                table: "ChapterVenues",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterVenues_ChapterId_VenueId",
                table: "ChapterVenues",
                columns: new[] { "ChapterId", "VenueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterVenues_Id",
                table: "ChapterVenues",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterVenues",
                table: "ChapterVenues");

            migrationBuilder.DropIndex(
                name: "IX_ChapterVenues_ChapterId_VenueId",
                table: "ChapterVenues");

            migrationBuilder.DropIndex(
                name: "IX_ChapterVenues_Id",
                table: "ChapterVenues");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterVenues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterVenues",
                table: "ChapterVenues",
                columns: new[] { "ChapterId", "VenueId" });
        }
    }
}
