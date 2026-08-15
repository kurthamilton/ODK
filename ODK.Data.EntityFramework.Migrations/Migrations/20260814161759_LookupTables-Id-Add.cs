using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class LookupTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Features",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               WHERE Id IS NULL so re-running fills only what is unset rather than rewriting every row.

               Wrapped in EXEC, and it has to be: the generated script puts the ALTER above and the UPDATE in
               one batch, and SQL Server binds column names when it parses a batch - so a plain UPDATE fails
               with "Invalid column name 'Id'" against a column the same batch is adding. EXEC defers parsing
               to execution. (Applying the migration directly runs each statement as its own batch and would
               not have shown this, but the script is what gets reviewed and deployed.) */
            migrationBuilder.Sql("EXEC('UPDATE Topics SET Id = TopicId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE TopicGroups SET Id = TopicGroupId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Features SET Id = FeatureId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Currencies SET Id = CurrencyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Countries SET Id = CountryId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TopicGroups");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Countries");
        }
    }
}
