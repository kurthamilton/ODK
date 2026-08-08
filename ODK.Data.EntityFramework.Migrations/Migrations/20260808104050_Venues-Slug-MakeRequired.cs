using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesSlugMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail loudly rather than let the column conversion paper over unbackfilled rows. The
            // scaffolded AlterColumn carried defaultValue: "", which emits an UPDATE turning every NULL
            // into an empty string - that satisfies NOT NULL, then either collides on the unique index
            // (two empties in one chapter) or leaves a venue with an empty slug in its URLs. Deleting
            // the default alone would surface it only as a bare SQL constraint error, so this says what
            // to do about it.
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM Venues WHERE Slug IS NULL OR Slug = '')
                    THROW 50000, 'Venues.Slug still has NULL or empty rows. Run the site admin "Backfill venue slugs" button on this database before applying this migration.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Venues",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ChapterId_Slug",
                table: "Venues",
                columns: new[] { "ChapterId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Venues_ChapterId_Slug",
                table: "Venues");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Venues",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
