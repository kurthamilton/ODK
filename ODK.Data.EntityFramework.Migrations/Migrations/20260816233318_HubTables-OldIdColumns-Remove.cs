using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns Members, Events and Chapters were keyed on before HubTables-Id-MakePrimaryKey moved
    /// them onto Id, finishing the rename of every identity column in the schema.
    /// </summary>
    /// <remarks>
    /// Written by hand: the mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since, and EF scaffolds an empty migration.
    /// </remarks>
    public partial class HubTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Any index still sitting on these columns would block the drop - EF guards only against a
               default constraint. These are the most heavily indexed tables in the schema, so the guard
               earns its place here even though no earlier batch needed it. */
            migrationBuilder.DropIndexes("Members", "MemberId");
            migrationBuilder.DropIndexes("Events", "EventId");
            migrationBuilder.DropIndexes("Chapters", "ChapterId");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "Chapters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: these columns were the key when this migration's Up ran,
               but putting that back is the previous migration's Down, not this one's. Going back through both
               returns the tables to where they started, and nothing is lost either way - the values are Id's. */
            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterId",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE Members SET MemberId = Id WHERE MemberId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Events SET EventId = Id WHERE EventId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Chapters SET ChapterId = Id WHERE ChapterId IS NULL')");
        }
    }
}
