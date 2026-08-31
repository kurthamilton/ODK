using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsReconciliationExcludedUtcRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReconciliationExcludedUtc",
                table: "Payments",
                newName: "ReconciliationIgnoredUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReconciliationIgnoredUtc",
                table: "Payments",
                newName: "ReconciliationExcludedUtc");
        }
    }
}
