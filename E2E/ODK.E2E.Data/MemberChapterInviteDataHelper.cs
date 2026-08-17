namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's outstanding chapter invitation directly from the database. A test can't open the
/// invitation email - there is no test email sink, and <c>SentEmails</c> records only the subject, not the
/// body - so the token is read here and the join URL built from it, the same compromise
/// <see cref="ActivationTokenDataHelper"/> makes for the activation link.
/// </summary>
public class MemberChapterInviteDataHelper : DataHelperBase
{
    public MemberChapterInviteDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Whether the member still holds an invitation to the chapter. Accepting consumes it, so this is how a
    /// test asserts that joining recorded the acceptance rather than leaving them permanently invited.
    /// </summary>
    public async Task<bool> HasInvite(string emailAddress, Guid chapterId)
    {
        const string sql =
            """
            SELECT COUNT(1)
            FROM MemberChapterInvites i
            INNER JOIN Members m ON m.Id = i.MemberId
            WHERE m.EmailAddress = @email AND i.ChapterId = @id
            """;

        await using var builder = Builder(sql)
            .AddParameter("@email", emailAddress)
            .AddParameter("@id", chapterId);

        return await builder.ExecuteScalar<int>() > 0;
    }

    /// <summary>
    /// The token from the invitation the member was emailed, which the join link carries.
    /// </summary>
    public async Task<string> GetInviteToken(string emailAddress, Guid chapterId)
    {
        const string sql =
            """
            SELECT TOP 1 i.Token
            FROM MemberChapterInvites i
            INNER JOIN Members m ON m.Id = i.MemberId
            WHERE m.EmailAddress = @email AND i.ChapterId = @id
            """;

        await using var builder = Builder(sql)
            .AddParameter("@email", emailAddress)
            .AddParameter("@id", chapterId);

        // The invitation is written by the import request itself, so it is there once the import has
        // redirected - but retry briefly in case the browser navigation wins the race with the commit.
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var token = await builder.ExecuteScalar<string>();

            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            await Task.Delay(250);
        }

        throw new InvalidOperationException(
            $"No invitation to chapter '{chapterId}' found for '{emailAddress}'. Did the import run?");
    }
}
