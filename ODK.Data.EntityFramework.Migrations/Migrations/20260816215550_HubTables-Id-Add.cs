using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class HubTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               EXEC because the generated script puts the ALTER above and the UPDATE in one batch, and SQL
               Server binds column names when it parses a batch - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE Members SET Id = MemberId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Events SET Id = EventId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Chapters SET Id = ChapterId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Chapters");
        }
    }
}
