using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class LegacyTablesRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Three tables from a design the app no longer runs: none is in the EF model, and nothing reads
               or writes them. Ticket purchases go through EventTicketPayments and EventResponses instead -
               see EventService.CompleteEventTicketPurchase, which touches neither of these tables - and
               PaymentReconciliation is a domain class with no mapping and no caller.

               Guarded throughout, because a database built from the migrations alone never had any of this:
               the tables pre-date the baseline and were never in the model to be scaffolded. */

            /* Payments carries an unmapped column referencing PaymentReconciliations, so the reference goes
               before the table it points at, and the foreign key and its index before the column - any index
               on a column blocks dropping it. */
            migrationBuilder.DropForeignKeys("Payments", "PaymentReconciliationId");
            migrationBuilder.DropIndexIfExists("Payments", "IX_Payments_PaymentReconciliationId");
            migrationBuilder.DropColumnIfExists("Payments", "PaymentReconciliationId");

            migrationBuilder.DropTableIfExists("ContactRequests");
            migrationBuilder.DropTableIfExists("EventTicketPurchases");
            migrationBuilder.DropTableIfExists("PaymentReconciliations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Deliberately not reversible. All three tables held rows, and no migration can bring data back;
               recreating the columns alone would report success while leaving the tables empty, which reads
               as a restore and is not one. A backup is the only way back. */
            throw new NotSupportedException(
                "LegacyTables-Remove cannot be reversed: ContactRequests, EventTicketPurchases and " +
                "PaymentReconciliations held rows when it ran. Restore them from a database backup.");
        }
    }
}
