using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenuesSlugAddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Versions any slug two chapters hold, oldest keeping the unversioned form. Production had
               none when this was written, and the build before this one already generates slugs unique
               across the site - but a venue created before that shipped can still collide, and an
               unguarded CREATE UNIQUE INDEX would fail the migration and block the deploy behind it.

               Repeated because one pass can create a collision of its own: "the-oak", "the-oak" and an
               existing "the-oak-2" turn the second into a duplicate of the third. Bounded, so an input
               it cannot settle fails the index creation instead of looping. */
            migrationBuilder.Sql(
                """
                DECLARE @pass int = 0;
                WHILE @pass < 10
                    AND EXISTS (SELECT 1 FROM [Venues] GROUP BY [Slug] HAVING COUNT(*) > 1)
                BEGIN
                    WITH [duplicates] AS (
                        SELECT [Slug],
                            ROW_NUMBER() OVER (PARTITION BY [Slug] ORDER BY [Id]) AS [version]
                        FROM [Venues])
                    UPDATE [duplicates]
                    SET [Slug] = LEFT([Slug], 255 - LEN(CAST([version] AS varchar(10))) - 1)
                        + '-' + CAST([version] AS varchar(10))
                    WHERE [version] > 1;

                    SET @pass += 1;
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Slug",
                table: "Venues",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Venues_Slug",
                table: "Venues");
        }
    }
}
