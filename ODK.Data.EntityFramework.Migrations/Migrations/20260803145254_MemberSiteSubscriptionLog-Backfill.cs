using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionLogBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill 0: the historic log rows predate the MemberId column (they were keyed only on the
            // payment), so populate MemberId from the payment. This must run first - the later steps key
            // the current record per member.
            migrationBuilder.Sql(@"
UPDATE l
SET l.MemberId = p.MemberId
FROM MemberSiteSubscriptionLog l
INNER JOIN Payments p ON p.PaymentId = l.PaymentId
WHERE l.MemberId IS NULL;");

            // Backfill A: for members who already have log records, flag the latest as the current one and
            // seed its expiry + external subscription id from the authoritative MemberSiteSubscriptions
            // snapshot. Older records keep ExpiresUtc = NULL (historic rolling expiry isn't reconstructable
            // and only the current expiry matters). The log row keeps its own SiteSubscriptionId. OUTER
            // APPLY (not a join) picks a single snapshot row deterministically - the snapshot is meant to be
            // one-per-member but isn't constrained, so a duplicate member would otherwise seed arbitrarily;
            // it also keeps a member with log history but no snapshot (NULL expiry = a non-expiring sub).
            migrationBuilder.Sql(@"
WITH Latest AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY MemberId ORDER BY CreatedUtc DESC, Id DESC) AS Rn
    FROM MemberSiteSubscriptionLog
    WHERE MemberId IS NOT NULL)
UPDATE l
SET l.IsCurrent = 1,
    l.ExpiresUtc = ms.ExpiresUtc,
    l.ExternalId = COALESCE(l.ExternalId, ms.ExternalId),
    l.SiteSubscriptionPriceId = COALESCE(l.SiteSubscriptionPriceId, ms.SiteSubscriptionPriceId)
FROM MemberSiteSubscriptionLog l
INNER JOIN Latest ON Latest.Id = l.Id AND Latest.Rn = 1
OUTER APPLY (
    SELECT TOP 1 s.ExpiresUtc, s.ExternalId, s.SiteSubscriptionPriceId
    FROM MemberSiteSubscriptions s
    WHERE s.MemberId = l.MemberId
    ORDER BY s.ExpiresUtc DESC) ms;");

            // Backfill B: members with a snapshot but no log row at all (free/default or comped
            // subscriptions - the bulk, created as a placeholder at account creation) get a synthetic
            // current record so the log becomes complete. No payment; just enough to carry the current
            // subscription, price, expiry and external id. A NULL expiry is a non-expiring (free) sub. The
            // ROW_NUMBER dedupes on MemberId (the snapshot isn't constrained to one row per member),
            // preferring the row with the furthest expiry, so a duplicate member gets exactly one current row.
            migrationBuilder.Sql(@"
INSERT INTO MemberSiteSubscriptionLog
    (Id, MemberId, SiteSubscriptionId, SiteSubscriptionPriceId, PaymentId, CreatedUtc, CancelledUtc,
     InitiatorId, ExternalId, ExpiresUtc, IsCurrent)
SELECT NEWID(), ms.MemberId, ms.SiteSubscriptionId, ms.SiteSubscriptionPriceId, NULL, GETUTCDATE(), NULL,
       NULL, ms.ExternalId, ms.ExpiresUtc, 1
FROM (
    SELECT s.*,
           ROW_NUMBER() OVER (PARTITION BY s.MemberId ORDER BY s.ExpiresUtc DESC) AS Rn
    FROM MemberSiteSubscriptions s) ms
WHERE ms.Rn = 1
  AND NOT EXISTS (
    SELECT 1 FROM MemberSiteSubscriptionLog l
    WHERE l.MemberId = ms.MemberId);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
