using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Removes the data created by the E2E tests. Test members are identified by the dedicated email
/// domain; member foreign keys are ON DELETE CASCADE, so deleting the member also removes its related
/// rows (activation token, password, preferences, site subscription, etc.). SentEmails has no foreign
/// key to Members, so its test rows (identified by recipient address) are removed explicitly.
/// </summary>
public static class TestDataCleaner
{
    public static async Task<int> DeleteTestData()
    {
        const string sql =
            """
            DELETE FROM SentEmails WHERE [To] LIKE @pattern;
            DELETE FROM Chapters WHERE OwnerId IN (SELECT MemberId FROM Members WHERE EmailAddress LIKE @pattern);
            DELETE FROM Members WHERE EmailAddress LIKE @pattern;
            """;

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@pattern", $"%@{TestAccounts.EmailDomain}");

        return await builder.ExecuteNonQuery();
    }
}