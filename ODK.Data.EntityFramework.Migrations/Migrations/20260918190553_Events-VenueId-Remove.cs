using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the column an event used to hold its venue in. Its index and foreign key went with
    /// <c>Events-Venue-UseChapterVenue</c>, which also unmapped it, so what is left is data nothing has
    /// read or written since that deploy.
    /// </summary>
    /// <remarks>
    /// Hand-written, because the model stopped describing this column a deploy ago and the scaffolder
    /// therefore has nothing to notice. A drop is its own migration for the same reason it is late: a
    /// column removed while a build that still writes it is serving takes that build down with it.
    /// </remarks>
    public partial class EventsVenueIdRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            /* Put back from the link, which has been the answer since the column stopped being one. Its
               index and foreign key belong to the migration before this and are restored by that one.

               Do not merge these two: a statement that names a column added in the same batch does not
               compile, so the backfill has to be its own. */
            migrationBuilder.Sql(
                """
                UPDATE e
                SET VenueId = cv.VenueId
                FROM Events e
                INNER JOIN ChapterVenues cv ON cv.Id = e.ChapterVenueId;
                """);
        }
    }
}
