using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Removes the data created by the E2E tests. Test members are identified by the dedicated email
/// domain; member foreign keys are ON DELETE CASCADE, so deleting the member also removes its related
/// rows (activation token, password, preferences, site subscription, etc.). SentEmails has no foreign
/// key to Members, so its test rows (identified by recipient address) are removed explicitly.
///
/// A chapter's child rows cascade when the chapter is deleted - including Venues, via their foreign key
/// to Chapters. Events are the exception: their foreign key to Chapters is RESTRICT (not cascade), so
/// events are deleted explicitly first (which also clears the Events -> Venues reference before the
/// chapter delete cascades the venues). Deleting an event cascades its own children (hosts, topics,
/// ticket settings, emails, responses, etc.). EventInvites is a further exception - its foreign key to
/// Events is RESTRICT - so any invites for the test events are removed before the events are deleted.
///
/// Site subscriptions a test creates (name prefixed <see cref="SiteSubscriptionDataHelper.TestNamePrefix"/>)
/// are not member-scoped, so they're removed explicitly - children (features, prices) first, then the
/// subscription - after the members, so any member subscription referencing one has already cascaded away.
/// The shared, reused default subscriptions (e.g. "ODK E2E Free") don't carry the prefix and are left alone.
/// </summary>
public class TestDataCleaner : DataHelperBase
{
    public TestDataCleaner(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<int> DeleteTestData()
    {
        const string memberIdSql = "SELECT MemberId FROM Members WHERE EmailAddress LIKE @pattern";
        const string siteSubIdSql = "SELECT SiteSubscriptionId FROM SiteSubscriptions WHERE Name LIKE @subPattern";

        const string sql =
            $"""
            DELETE FROM SentEmails WHERE [To] LIKE @pattern;

            DELETE ei FROM EventInvites ei
                INNER JOIN Events e ON e.EventId = ei.EventId
                INNER JOIN Chapters c ON c.ChapterId = e.ChapterId
                WHERE c.OwnerId IN ({memberIdSql});

            DELETE e FROM Events e
                INNER JOIN Chapters c ON c.ChapterId = e.ChapterId
                WHERE c.OwnerId IN ({memberIdSql});

            DELETE FROM ChapterPaymentAccounts WHERE ChapterId IN
                (SELECT ChapterId FROM Chapters WHERE OwnerId IN ({memberIdSql}));
            DELETE FROM MemberSubscriptionLog WHERE MemberId IN ({memberIdSql});
            DELETE FROM ChapterSubscriptions WHERE ChapterId IN
                (SELECT ChapterId FROM Chapters WHERE OwnerId IN ({memberIdSql}));
            DELETE FROM Chapters WHERE OwnerId IN ({memberIdSql});
            DELETE FROM MemberSiteSubscriptionLog WHERE MemberId IN ({memberIdSql});
            DELETE FROM Payments WHERE MemberId IN ({memberIdSql});
            DELETE FROM MemberSiteSubscriptions WHERE MemberId IN ({memberIdSql});
            DELETE FROM Members WHERE MemberId IN ({memberIdSql});

            DELETE FROM SiteSubscriptionFeatures WHERE SiteSubscriptionId IN ({siteSubIdSql});
            DELETE FROM SiteSubscriptionPrices WHERE SiteSubscriptionId IN ({siteSubIdSql});
            DELETE FROM SiteSubscriptions WHERE SiteSubscriptionId IN ({siteSubIdSql});
            """;

        await using var builder = Builder(sql)
            .AddParameter("@pattern", $"%@{TestAccounts.EmailDomain}")
            .AddParameter("@subPattern", $"{SiteSubscriptionDataHelper.TestNamePrefix}%");

        return await builder.ExecuteNonQuery();
    }
}