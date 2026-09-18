using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Removes the data created by the E2E tests. Test members are identified by the dedicated email
/// domain; member foreign keys are ON DELETE CASCADE, so deleting the member also removes its related
/// rows (activation token, password, preferences, site subscription, etc.). SentEmails has no foreign
/// key to Members, so its test rows (identified by recipient address) are removed explicitly.
///
/// A chapter's child rows cascade when the chapter is deleted, but venues and events do not. Events'
/// foreign key to Chapters is RESTRICT (not cascade), so events are deleted explicitly first, which also
/// clears the Events -> Venues reference. Deleting an event cascades its own children (hosts, topics,
/// ticket settings, emails, responses, etc.). EventInvites is a further exception - its foreign key to
/// Events is RESTRICT - so any invites for the test events are removed before the events are deleted.
/// Venues belong to the site rather than to a chapter, so nothing removes them when their chapter goes -
/// but a venue is shared, and the same place reached twice is one row however many chapters link to it, so
/// a test can be sitting on a venue somebody real created. Deleting it would cascade away their link and
/// its location too. So venues go last, once the test chapters and their links have gone, and only where
/// nothing links to them any more <em>and</em> they were created during this run: the first condition
/// leaves anything real alone, the second leaves orphans that were already here alone. A test venue that
/// survives both is litter, which is the safe way to be wrong.
///
/// Site subscriptions a test creates (name prefixed <see cref="SiteSubscriptionDataHelper.TestNamePrefix"/>)
/// are not member-scoped, so they're removed explicitly - children (features, prices) first, then the
/// subscription - after the members, so any member subscription referencing one has already cascaded away.
/// The shared, reused default subscriptions (e.g. "ODK E2E Free") don't carry the prefix and are left alone.
///
/// Site questions are likewise not member-scoped - they belong to a platform - so test rows (name prefixed
/// <see cref="SiteQuestionDataHelper.TestNamePrefix"/>) are removed explicitly. They have no children.
///
/// Site conversations are the one thing a test does that writes rows belonging to somebody else: writing to
/// the site notifies and emails every site admin, real ones included, so those rows are outside the member
/// cascade. The notifications are found through the conversation they point at (<c>Notifications.EntityId</c>),
/// which means removing them <em>before</em> the members - once the member goes, so does the conversation that
/// identifies them. Their emails have no foreign key to be found by, only
/// <see cref="TestSiteConversations.SubjectPrefix"/> carried through the subject.
/// </summary>
public class TestDataCleaner : DataHelperBase
{
    public TestDataCleaner(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<int> DeleteTestData(DateTime runStartUtc)
    {
        const string memberIdSql = "SELECT Id FROM Members WHERE EmailAddress LIKE @pattern";
        const string siteSubIdSql = "SELECT Id FROM SiteSubscriptions WHERE Name LIKE @subPattern";

        const string sql =
            $"""
            DELETE FROM SentEmails WHERE [To] LIKE @pattern;
            DELETE FROM SentEmails WHERE Subject LIKE @conversationSubjectPattern;

            DELETE ei FROM EventInvites ei
                INNER JOIN Events e ON e.Id = ei.EventId
                INNER JOIN Chapters c ON c.Id = e.ChapterId
                WHERE c.OwnerId IN ({memberIdSql});

            DELETE e FROM Events e
                INNER JOIN Chapters c ON c.Id = e.ChapterId
                WHERE c.OwnerId IN ({memberIdSql});

            DELETE FROM ChapterPaymentAccounts WHERE ChapterId IN
                (SELECT Id FROM Chapters WHERE OwnerId IN ({memberIdSql}));
            DELETE FROM MemberSubscriptionLog WHERE MemberId IN ({memberIdSql});
            DELETE FROM ChapterSubscriptions WHERE ChapterId IN
                (SELECT Id FROM Chapters WHERE OwnerId IN ({memberIdSql}));
            DELETE FROM Chapters WHERE OwnerId IN ({memberIdSql});
            DELETE FROM MemberSiteSubscriptionLog WHERE MemberId IN ({memberIdSql});
            DELETE FROM Payments WHERE MemberId IN ({memberIdSql});
            DELETE FROM Notifications WHERE EntityId IN
                (SELECT Id FROM SiteConversations WHERE MemberId IN ({memberIdSql}));
            DELETE FROM Members WHERE Id IN ({memberIdSql});

            DELETE FROM Venues
            WHERE CreatedUtc >= @runStartUtc
                AND NOT EXISTS (SELECT 1 FROM ChapterVenues cv WHERE cv.VenueId = Venues.Id)
                AND NOT EXISTS (SELECT 1 FROM Events e WHERE e.VenueId = Venues.Id);

            DELETE FROM SiteSubscriptionFeatures WHERE SiteSubscriptionId IN ({siteSubIdSql});
            DELETE FROM SiteSubscriptionPrices WHERE SiteSubscriptionId IN ({siteSubIdSql});
            DELETE FROM SiteSubscriptions WHERE Id IN ({siteSubIdSql});

            DELETE FROM SiteQuestions WHERE Name LIKE @questionPattern;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@pattern", $"%@{TestAccounts.EmailDomain}")
            .AddParameter("@runStartUtc", runStartUtc)
            .AddParameter("@subPattern", $"{SiteSubscriptionDataHelper.TestNamePrefix}%")
            .AddParameter("@questionPattern", $"{SiteQuestionDataHelper.TestNamePrefix}%")
            .AddParameter("@conversationSubjectPattern", $"%{TestSiteConversations.SubjectPrefix}%");

        return await builder.ExecuteNonQuery();
    }
}