namespace ODK.E2E.Data;

/// <summary>
/// Reads the subjects of the emails recorded as sent to an address, straight from the database. Every
/// email that passes through the email client - including the e2e <c>ConsoleEmailClient</c> - is
/// written to <c>SentEmails</c> immediately after a successful send, so this is a faithful record of
/// what the client "sent" without needing to scrape the app's console output.
/// </summary>
public static class SentEmailDataHelper
{
    public static async Task<IReadOnlyCollection<string>> GetSubjects(string emailAddress, int expectedCount)
    {
        const string sql =
            """
            SELECT Subject
            FROM SentEmails
            WHERE [To] = @email
            ORDER BY SentDate
            """;

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@email", emailAddress);

        // Emails are sent by a background (Hangfire) job that runs after the request commits, so poll
        // until the expected number of rows appears; give up and return what we have so the assertion
        // can report the shortfall.
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var subjects = await builder.ReadMany(x => x.GetString(0));
            if (subjects.Count >= expectedCount)
            {
                return subjects;
            }

            await Task.Delay(250);
        }

        return [];
    }
}