using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// An event's venue becomes the link it was booked through. <c>ChapterVenueId</c> is filled in from
    /// the pair that used to say the same thing and made required; <c>VenueId</c> is made nullable and
    /// left in place, to be dropped once a build that never reads it is the only one running. Its index
    /// and foreign key go now, though - see below.
    /// </summary>
    /// <remarks>
    /// The order matters in three places. The backfill has to read <c>VenueId</c> while it is still the
    /// answer. The index on <c>ChapterVenueId</c> has to go before the column can be made required -
    /// SQL Server refuses to alter a column an index depends on - and comes back after. And
    /// <c>VenueId</c> has to become nullable in the same migration that unmaps it: the column stays
    /// behind with no default, so the first insert from a build that has stopped writing it would
    /// otherwise fail.
    /// </remarks>
    public partial class EventsVenueUseChapterVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Every event, not only the ones holding null: updating an event's venue set VenueId without
               setting ChapterVenueId, so a row can carry a link to somewhere it is no longer held. */
            migrationBuilder.Sql(
                """
                UPDATE e
                SET ChapterVenueId = cv.Id
                FROM Events e
                INNER JOIN ChapterVenues cv
                    ON cv.ChapterId = e.ChapterId AND cv.VenueId = e.VenueId
                WHERE e.ChapterVenueId IS NULL OR e.ChapterVenueId <> cv.Id;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterVenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events",
                column: "ChapterVenueId");

            /* The column is kept so the build running while this applies carries on writing it, and so
               what an event used to point at is still readable. Its constraints are not: nothing maps it
               from here, so nothing keeps it in step - changing an event's venue updates the link and
               leaves this behind - and a stale foreign key no code knows about is a venue that cannot be
               deleted for a reason nothing on the screen can explain. */
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_VenueId",
                table: "Events");

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Events recorded since have no VenueId, so it is put back from the link before it is required.
            migrationBuilder.Sql(
                """
                UPDATE e
                SET VenueId = cv.VenueId
                FROM Events e
                INNER JOIN ChapterVenues cv ON cv.Id = e.ChapterVenueId
                WHERE e.VenueId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterVenueId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ChapterVenueId",
                table: "Events",
                column: "ChapterVenueId");
        }
    }
}
