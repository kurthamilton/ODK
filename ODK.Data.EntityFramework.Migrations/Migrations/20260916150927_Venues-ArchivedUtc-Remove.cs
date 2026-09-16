using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesArchivedUtcRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Archiving is per chapter and lives on ChapterVenues. See Venues-ChapterId-Remove for why
               a drop waits a deploy, and why IfExists. */
            migrationBuilder.DropColumnIfExists("Venues", "ArchivedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The column comes back empty; the archive dates are on ChapterVenues.
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedUtc",
                table: "Venues",
                type: "datetime2",
                nullable: true);
        }
    }
}
