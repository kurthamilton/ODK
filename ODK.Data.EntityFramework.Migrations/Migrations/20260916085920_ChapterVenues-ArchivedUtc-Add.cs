using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterVenuesArchivedUtcAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedUtc",
                table: "ChapterVenues",
                type: "datetime2",
                nullable: true);

            /* Carry each venue's archive state onto its chapter's link. Archiving is the chapter's from
               here, and a venue archived before this ran was archived by the only chapter that had it. */
            migrationBuilder.Sql(
                """
                UPDATE [cv]
                SET [cv].[ArchivedUtc] = [v].[ArchivedUtc]
                FROM [ChapterVenues] [cv]
                INNER JOIN [Venues] [v] ON [v].[Id] = [cv].[VenueId]
                WHERE [v].[ArchivedUtc] IS NOT NULL AND [cv].[ArchivedUtc] IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedUtc",
                table: "ChapterVenues");
        }
    }
}
