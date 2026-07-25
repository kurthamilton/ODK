namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's pending activation token directly from the database - an end-to-end test can't
/// open the activation email, and there is no test email sink.
/// </summary>
public class ActivationTokenDataHelper : DataHelperBase
{
    public ActivationTokenDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<string> GetActivationToken(string emailAddress)
    {
        const string sql =
            """
            SELECT TOP 1 t.ActivationToken
            FROM MemberActivationTokens t
            INNER JOIN Members m ON m.MemberId = t.MemberId
            WHERE m.EmailAddress = @email
            """;

        await using var builder = Builder(sql)
            .AddParameter("@email", emailAddress);

        // The token is written before the "check your email" redirect, but retry briefly in case the
        // browser navigation completes before the transaction is visible.
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
            $"No activation token found for '{emailAddress}'. Is the app pointed at the same database as ODK_E2E_CONNECTION_STRING?");
    }
}