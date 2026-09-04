using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChaptersBannerImageUrlDrop : Migration
    {
        /* Written by hand: the migration before this one took BannerImageUrl out of the model, so there is
           no model change left to scaffold from. DropColumn rather than ColumnSql.Drop, because the column
           came from InitialCreate and so exists in every database, restored or built from the migrations. */

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Chapters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* The paths are not recovered, and must not be: they addressed files under
               wwwroot/assets/{chapter} that were deleted along with the readers, so a rebuilt path would
               resolve to nothing and render a broken image where an absent one renders nothing at all. A
               chapter's picture lives in ChapterHeaderImages, which this leaves untouched. */
            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Chapters",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
