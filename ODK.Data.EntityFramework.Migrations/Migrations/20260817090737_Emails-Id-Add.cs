using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* First of three releases moving this table's key from EmailTypeId to Id. The column is added and
               filled here and nothing maps it, so the build running while this applies neither knows nor needs
               to know about it - which is what makes the window safe. Hand-written: the model does not change,
               so there is nothing to scaffold.

               Nullable, because the rows already exist. Made NOT NULL once it holds values, in the release that
               moves the key onto it. */

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Emails",
                nullable: true);

            /* Wrapped in EXEC because SQL Server binds column names when it parses a batch, and the column this
               assigns to is created by the statement above it in that same batch. */
            migrationBuilder.Sql("EXEC(N'UPDATE [Emails] SET [Id] = [EmailTypeId];');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Emails");
        }
    }
}
