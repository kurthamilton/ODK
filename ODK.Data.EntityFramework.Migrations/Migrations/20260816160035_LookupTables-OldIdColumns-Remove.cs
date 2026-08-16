using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns the lookup tables were keyed on before LookupTables-Id-MakePrimaryKey moved them
    /// onto Id. Nothing has read or written them since the build that migration shipped with.
    /// </summary>
    /// <remarks>
    /// Written by hand. The mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since - EF diffs the model against the snapshot, sees no difference, and
    /// scaffolds an empty migration. Expect the same for the last phase of every batch.
    /// </remarks>
    public partial class LookupTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "TopicGroupId",
                table: "TopicGroups");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Countries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: the column was the key when this migration's Up ran, but
               putting that back is the previous migration's Down, not this one's. Going back through both
               returns the table to where it started, and nothing is lost either way - the values are Id's. */
            migrationBuilder.AddColumn<Guid>(
                name: "TopicId",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TopicGroupId",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId",
                table: "Features",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE Topics SET TopicId = Id WHERE TopicId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE TopicGroups SET TopicGroupId = Id WHERE TopicGroupId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Features SET FeatureId = Id WHERE FeatureId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Currencies SET CurrencyId = Id WHERE CurrencyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Countries SET CountryId = Id WHERE CountryId IS NULL')");
        }
    }
}
