using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesChapterIdRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* A chapter reaches its venues through ChapterVenues, so the column has nothing left to
               say. Unmapped one deploy ago, which is what makes dropping it safe: the build that was
               still selecting it has been replaced.

               DropColumnIfExists rather than DropColumn - it clears the column's default constraint,
               looked up rather than named, and is a no-op in a database that never had the column. */
            migrationBuilder.DropColumnIfExists("Venues", "ChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* The column comes back empty. Which chapter each venue belonged to is recoverable from
               ChapterVenues, but only while every venue still has exactly one link. */
            migrationBuilder.AddColumn<Guid>(
                name: "ChapterId",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
