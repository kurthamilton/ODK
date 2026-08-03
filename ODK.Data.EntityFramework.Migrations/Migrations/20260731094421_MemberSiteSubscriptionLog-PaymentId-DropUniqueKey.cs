using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    public partial class MemberSiteSubscriptionLogPaymentIdDropUniqueKey : Migration
    {
        // UQ_MemberSiteSubscriptionLog_PaymentId is a manual (non-EF) unique key added before this table
        // became a log - hence the raw SQL and "UQ_" prefix, and why this migration has no model change.
        // Under the old model there was one row per payment, so PaymentId was unique. The table is now an
        // append-only log: placeholder/confirm/admin records carry no payment (NULL PaymentId), and recurring
        // renewals reuse the original checkout payment - so many rows legitimately share a PaymentId (or NULL,
        // which SQL Server's unique key still rejects beyond the first). The uniqueness must go. Nothing
        // queries by PaymentId, so no replacement index is created.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop whether it was created as a unique key constraint or a bare unique index.
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_MemberSiteSubscriptionLog_PaymentId') " +
                "ALTER TABLE MemberSiteSubscriptionLog DROP CONSTRAINT UQ_MemberSiteSubscriptionLog_PaymentId;");
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS UQ_MemberSiteSubscriptionLog_PaymentId ON MemberSiteSubscriptionLog;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort restore; will fail if the log has since gained duplicate or multiple NULL PaymentId
            // rows (which the new model expects).
            migrationBuilder.Sql(
                "ALTER TABLE MemberSiteSubscriptionLog " +
                "ADD CONSTRAINT UQ_MemberSiteSubscriptionLog_PaymentId UNIQUE (PaymentId);");
        }
    }
}
