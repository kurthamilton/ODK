using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesChapterIdRemoveForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* A chapter reaches its venues through ChapterVenues from here on, and that table cascades
               from Chapters. It cannot also be reached through Venues - SQL Server refuses a second
               cascade path - so this foreign key goes before that table is created, and deleting a
               chapter stops deleting its venues.

               Dropped by column: the scaffolder can only guess a constraint's name, and much of this
               schema pre-dates EF. */
            migrationBuilder.DropForeignKeys("Venues", "ChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
