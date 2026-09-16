using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesPrimaryKeyMakeClustered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* The primary key cannot be dropped while anything references it, and the table cannot take
               a clustered key while CIX_Venues_ChapterId holds that role. Both go, and the three
               foreign keys come back with the delete behaviour they had - DeleteVenue is a single
               delete only because ChapterVenues and VenueLocations cascade, and is refused for a venue
               with events only because Events does not.

               Dropped by column and by existence check: these constraints pre-date EF in a restored
               database and never existed in one built from the migrations. */
            migrationBuilder.DropForeignKeys("ChapterVenues", "VenueId");
            migrationBuilder.DropForeignKeys("Events", "VenueId");
            migrationBuilder.DropForeignKeys("VenueLocations", "VenueId");

            migrationBuilder.DropPrimaryKeyIfExists("Venues");
            migrationBuilder.DropIndexIfExists("Venues", "CIX_Venues_ChapterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venues",
                table: "Venues",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterVenues_Venues_VenueId",
                table: "ChapterVenues",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueLocations_Venues_VenueId",
                table: "VenueLocations",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Venues",
                table: "Venues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venues",
                table: "Venues",
                column: "Id");
        }
    }
}
