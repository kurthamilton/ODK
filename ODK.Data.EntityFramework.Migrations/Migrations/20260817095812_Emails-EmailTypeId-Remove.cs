using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsEmailTypeIdRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Last of three releases moving this table's key from EmailTypeId to Id. The live build maps Type to
               Id and never reads this column. Hand-written: the model stopped mentioning EmailTypeId when the
               mapping moved, so there is nothing left for the scaffolder to notice.

               DropColumn, not DropColumnIfExists: InitialCreate created this column, so every database has it.
               The guarded form is for a column no migration ever created. */
            migrationBuilder.DropColumn(
                name: "EmailTypeId",
                table: "Emails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and carrying no key: the release that moved the key off this column is the one that puts
               that back, so this only has to restore the column and its values. */
            migrationBuilder.AddColumn<int>(
                name: "EmailTypeId",
                table: "Emails",
                nullable: true);

            /* Wrapped in EXEC because SQL Server binds column names when it parses a batch, and the column this
               assigns to is created by the statement above it in that same batch. */
            migrationBuilder.Sql("EXEC(N'UPDATE [Emails] SET [EmailTypeId] = [Id];');");
        }
    }
}
