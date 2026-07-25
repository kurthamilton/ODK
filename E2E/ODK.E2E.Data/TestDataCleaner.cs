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
/// </summary>
public class TestDataCleaner : DataHelperBase
{
    public TestDataCleaner(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<int> DeleteTestData()
    {
        const string sql =
            """
            DELETE FROM SentEmails WHERE [To] LIKE @pattern;

            DELETE ei FROM EventInvites ei
                INNER JOIN Events e ON e.EventId = ei.EventId
                INNER JOIN Chapters c ON c.ChapterId = e.ChapterId
                WHERE c.OwnerId IN (SELECT MemberId FROM Members WHERE EmailAddress LIKE @pattern);

            DELETE e FROM Events e
                INNER JOIN Chapters c ON c.ChapterId = e.ChapterId
                WHERE c.OwnerId IN (SELECT MemberId FROM Members WHERE EmailAddress LIKE @pattern);

            DELETE FROM Chapters WHERE OwnerId IN (SELECT MemberId FROM Members WHERE EmailAddress LIKE @pattern);
            DELETE FROM Members WHERE EmailAddress LIKE @pattern;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@pattern", $"%@{TestAccounts.EmailDomain}");

        return await builder.ExecuteNonQuery();
    }
}