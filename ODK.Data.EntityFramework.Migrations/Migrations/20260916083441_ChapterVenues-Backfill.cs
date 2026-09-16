using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterVenuesBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* The same statement ChapterVenues-Add ran. A venue created between that migration and the
               build that writes the link row itself has none, and this deploy is the one that starts
               reading venues through the table, so every venue needs its link before it lands. A
               database with nothing outstanding gets a no-op. */
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
            /* Nothing to undo: the rows this inserts are indistinguishable from the ones the table was
               created with, and removing them would take those with it. */
        }
    }
}
