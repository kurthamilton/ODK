using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesPlacesColumnsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Existing venues get the moment this runs: they were created before anyone recorded when,
               and a made-up spread of dates would look like data. Ordering among them falls back to the
               id, which ascends in creation order because SequentialIdGenerator mints it that way - so a
               query wanting the latest record of a place orders by CreatedUtc then Id. */
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Venues",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "VenueLocations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "ChapterVenues",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueLocations_ExternalId",
                table: "VenueLocations",
                column: "ExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VenueLocations_ExternalId",
                table: "VenueLocations");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "VenueLocations");

            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "ChapterVenues");
        }
    }
}
