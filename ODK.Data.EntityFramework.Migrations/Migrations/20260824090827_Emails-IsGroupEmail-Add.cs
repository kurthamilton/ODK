using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsIsGroupEmailAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Not a RenameColumn, though that is what the model change scaffolds: migrations are applied
               before the release that maps the new name, so the outgoing one has to keep reading the old
               column. Emails is read on every send, not only by the admin form, so a rename takes email
               down for the length of the deploy. Overridable is left in place for a later migration to
               drop, once nothing maps it. */
            migrationBuilder.AddColumn<bool>(
                name: "IsGroupEmail",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Emails SET IsGroupEmail = Overridable;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Overridable still exists and is what the reverted code reads, so it takes the flag back.
            migrationBuilder.Sql("UPDATE Emails SET Overridable = IsGroupEmail;");

            migrationBuilder.DropColumn(
                name: "IsGroupEmail",
                table: "Emails");
        }
    }
}
