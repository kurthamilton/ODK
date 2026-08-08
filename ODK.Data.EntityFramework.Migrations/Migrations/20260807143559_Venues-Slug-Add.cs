using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesSlugAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the pre-existing Chapter/Venue name and slug lengths and indexes that were missing from EF

            // Drop the manually-generated indexes if they exist
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Venues_ChapterId_Name') " +
                "ALTER TABLE Venues DROP CONSTRAINT UQ_Venues_ChapterId_Name;");
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Chapters_Slug') " +
                "ALTER TABLE Chapters DROP CONSTRAINT UQ_Chapters_Slug;");
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Chapters_Name') " +
                "ALTER TABLE Chapters DROP CONSTRAINT UQ_Chapters_Name;");

            // Drop the EF-generated index if it exists
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Venues_ChapterId ON Venues;");

            // Fix column widths in a DB built entirely from EF. Prod columns were already 255
            migrationBuilder.AlterColumn<string>(
                name: "Name", table: "Chapters", type: "nvarchar(255)", maxLength: 255, nullable: false,
                oldClrType: typeof(string), oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Slug", table: "Chapters", type: "nvarchar(255)", maxLength: 255, nullable: false,
                oldClrType: typeof(string), oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name", table: "Venues", type: "nvarchar(255)", maxLength: 255, nullable: false,
                oldClrType: typeof(string), oldType: "nvarchar(max)");

            // Let EF generate its default indexes
            migrationBuilder.CreateIndex(
                name: "IX_Venues_ChapterId_Name",
                table: "Venues",
                columns: new[] { "ChapterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_Name",
                table: "Chapters",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_Slug",
                table: "Chapters",
                column: "Slug",
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Venues",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Venues");
        }
    }
}
