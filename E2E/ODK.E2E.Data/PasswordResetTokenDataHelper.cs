namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's pending password-reset token from the database - an end-to-end test can't open the
/// reset email, and there is no test email sink. The row is written before the "check your email"
/// response, so this reads the committed row with a short retry.
/// </summary>
public class PasswordResetTokenDataHelper : DataHelperBase
{
    public PasswordResetTokenDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// The most recent outstanding token for the member. Newest first because requesting a reset twice
    /// leaves two rows, and only the latest is the one an email just carried.
    /// </summary>
    public async Task<string> GetToken(Guid memberId)
    {
        const string sql =
            """
            SELECT TOP 1 Token
            FROM MemberPasswordResetRequests
            WHERE MemberId = @memberId
            ORDER BY CreatedUtc DESC
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId);

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
            $"No password reset token found for member '{memberId}'. Is the app pointed at the same " +
            "database as ODK_E2E_CONNECTION_STRING?");
    }
}
