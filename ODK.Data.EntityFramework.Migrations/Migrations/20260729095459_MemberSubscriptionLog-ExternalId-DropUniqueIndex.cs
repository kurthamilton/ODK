using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSubscriptionLogExternalIdDropUniqueIndex : Migration
    {
        // The ExternalId index is a manual (non-EF) index - hence the raw SQL and "UQ_" prefix, and why this
        // migration has no model change. Under the old model there was one log record per subscription, so
        // ExternalId (the payment provider subscription id) was unique. Renewals now append a record per
        // event, all sharing that subscription id, so the uniqueness must go. A non-unique index replaces it
        // to keep the cancellation lookup (GetLatestByExternalIdOrDefault) fast.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS UQ_MemberSubscriptionLog_ExternalId ON MemberSubscriptionLog;");
            migrationBuilder.Sql(
                "CREATE INDEX IX_MemberSubscriptionLog_ExternalId ON MemberSubscriptionLog (ExternalId) " +
                "WHERE ExternalId IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_MemberSubscriptionLog_ExternalId ON MemberSubscriptionLog;");
            // Best-effort restore; will fail if renewals have already appended duplicate ExternalId rows.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX UQ_MemberSubscriptionLog_ExternalId ON MemberSubscriptionLog (ExternalId) " +
                "WHERE ExternalId IS NOT NULL;");
        }
    }
}
