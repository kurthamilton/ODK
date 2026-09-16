using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterVenuesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChapterVenues",
                columns: table => new
                {
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterVenues", x => new { x.ChapterId, x.VenueId });
                    table.ForeignKey(
                        name: "FK_ChapterVenues_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChapterVenues_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterVenues_VenueId",
                table: "ChapterVenues",
                column: "VenueId");

            /* Every existing venue is used by the chapter that owns it. The guard makes the statement
               repeatable: a venue created between this migration and the build that writes the link row
               itself has none, and running this again picks it up. */
            migrationBuilder.Sql(
                """
                INSERT INTO [ChapterVenues] ([ChapterId], [VenueId])
                SELECT [v].[ChapterId], [v].[Id]
                FROM [Venues] [v]
                WHERE NOT EXISTS (
                    SELECT 1 FROM [ChapterVenues] [cv]
                    WHERE [cv].[VenueId] = [v].[Id] AND [cv].[ChapterId] = [v].[ChapterId])
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterVenues");
        }
    }
}
