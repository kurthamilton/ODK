using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsOverridableRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Second of two releases renaming this column to IsGroupEmail. The live build maps IsGroupEmail
               and never reads this one. Hand-written: the model stopped mentioning Overridable when
               Emails-IsGroupEmail-Add landed, so there is nothing left for the scaffolder to notice.

               DropColumn, not DropColumnIfExists: InitialCreate created this column, so every database has
               it. DropColumn also clears the default constraint that would otherwise block the drop, which
               matters here - a restored database names that constraint DF__Emails__Overrida__<hash>, so
               nothing could drop it by name. */
            migrationBuilder.DropColumn(
                name: "Overridable",
                table: "Emails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Lossless: IsGroupEmail still carries the flag, so the restored column takes its values from
               there. The default only fills the rows in between. */
            migrationBuilder.AddColumn<bool>(
                name: "Overridable",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Emails SET Overridable = IsGroupEmail;");
        }
    }
}
