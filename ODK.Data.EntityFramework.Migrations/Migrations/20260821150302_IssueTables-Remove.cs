using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class IssueTablesRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Dropping a table takes its own foreign keys with it, so only the order matters here:
               IssueMessages is the only table referencing Issues, and nothing outside these four references
               any of them. */
            migrationBuilder.DropTable(
                name: "IssueMessages");

            migrationBuilder.DropTable(
                name: "Issues");

            /* The two lookup tables are not in the EF model, so nothing scaffolds them. Guarded because only
               a database restored from production has them - one built from the migrations alone never did. */
            migrationBuilder.DropTableIfExists("IssueStatusTypes");
            migrationBuilder.DropTableIfExists("IssueTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Deliberately not reversible. Issues and IssueMessages held rows, and no migration can bring
               data back; recreating the four tables would report success while leaving them empty, which
               reads as a restore and is not one. A backup is the only way back. */
            throw new NotSupportedException(
                "IssueTables-Remove cannot be reversed: Issues and IssueMessages held rows when it ran. " +
                "Restore them from a database backup.");
        }
    }
}
