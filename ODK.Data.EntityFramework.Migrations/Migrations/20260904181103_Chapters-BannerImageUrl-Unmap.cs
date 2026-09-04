using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChaptersBannerImageUrlUnmap : Migration
    {
        /* No schema change. The model has stopped mapping Chapters.BannerImageUrl, whose job the
           ChapterHeaderImages table now does, and a model the migrations do not account for is refused at
           the point they are applied - so the change is recorded here to keep the deploy that ships it
           applicable.

           The column is dropped by the migration after this one, which cannot come any sooner: a migration
           runs a minute ahead of the code it ships with, so a column dropped here would go while the build
           that still selects it is serving. Scaffolding wrote that DropColumn here; it is deleted
           deliberately. */

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
