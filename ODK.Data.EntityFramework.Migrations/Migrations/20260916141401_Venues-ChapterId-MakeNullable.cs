using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesChapterIdMakeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Venues_ChapterId_Name",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_ChapterId_Slug",
                table: "Venues");

            /* Both columns leave the model here and the table in the migration after, because the build
               this ships with is not the only one selecting them: the previous build serves on for about
               a minute after this runs. ArchivedUtc is already nullable; ChapterId is not, and an insert
               from the new build omits it, so it has to accept a null before that build arrives. */
            migrationBuilder.Sql("ALTER TABLE [Venues] ALTER COLUMN [ChapterId] uniqueidentifier NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Only the nullability and the indexes: Up leaves both columns in place, so there is nothing
               to add back. Fails once any venue has been created without a chapter, which is the point
               at which this is no longer reversible. */
            migrationBuilder.Sql("ALTER TABLE [Venues] ALTER COLUMN [ChapterId] uniqueidentifier NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ChapterId_Name",
                table: "Venues",
                columns: new[] { "ChapterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ChapterId_Slug",
                table: "Venues",
                columns: new[] { "ChapterId", "Slug" },
                unique: true);
        }
    }
}
