using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenueChapterIdAddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*Remove orphaned rows*/
            const string sql =
                "DELETE Venues " +
                "WHERE NOT EXISTS(SELECT * FROM Chapters WHERE Chapters.ChapterId = Venues.ChapterId);";
            migrationBuilder.Sql(sql);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ChapterId",
                table: "Venues",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_ChapterId",
                table: "Venues");
        }
    }
}