using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSubscriptionLogBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill A: for members who already have log records, flag the latest per (member, chapter)
            // as the current one and seed its expiry from the authoritative MemberSubscriptions snapshot.
            // Older records keep ExpiresUtc = NULL (historic rolling expiry isn't reconstructable and only
            // the current expiry matters). LEFT JOIN so a member with log history but no snapshot still gets
            // a current record (with a NULL expiry) rather than being dropped.
            migrationBuilder.Sql(@"
WITH Latest AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY MemberId, ChapterId ORDER BY PurchasedUtc DESC, Id DESC) AS Rn
    FROM MemberSubscriptionLog)
UPDATE l
SET l.IsCurrent = 1,
    l.ExpiresUtc = ms.ExpiresUtc
FROM MemberSubscriptionLog l
INNER JOIN Latest ON Latest.Id = l.Id AND Latest.Rn = 1
LEFT JOIN MemberChapters mc ON mc.MemberId = l.MemberId AND mc.ChapterId = l.ChapterId
LEFT JOIN MemberSubscriptions ms ON ms.MemberChapterId = mc.MemberChapterId;");

            // Backfill B: members with a snapshot but no log row at all (trial / comped memberships) get a
            // synthetic current record so the log becomes complete. PurchasedUtc = the membership's start,
            // Amount/Months = 0, no payment - just enough to carry the current subscription type and expiry.
            migrationBuilder.Sql(@"
INSERT INTO MemberSubscriptionLog
    (Id, MemberId, ChapterId, ChapterSubscriptionId, PaymentId, PurchasedUtc, CancelledUtc,
     Amount, Months, SubscriptionTypeId, InitiatorId, ExternalId, ExpiresUtc, IsCurrent)
SELECT NEWID(), mc.MemberId, mc.ChapterId, NULL, NULL, mc.CreatedUtc, NULL,
       0, 0, ms.SubscriptionTypeId, NULL, NULL, ms.ExpiresUtc, 1
FROM MemberSubscriptions ms
INNER JOIN MemberChapters mc ON mc.MemberChapterId = ms.MemberChapterId
WHERE NOT EXISTS (
    SELECT 1 FROM MemberSubscriptionLog l
    WHERE l.MemberId = mc.MemberId AND l.ChapterId = mc.ChapterId);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
