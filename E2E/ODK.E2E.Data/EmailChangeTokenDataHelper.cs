namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's pending email-change confirmation token from the database - an end-to-end test can't
/// open the confirmation email, and there is no test email sink. The token is written when the change is
/// requested (before the response), so this reads the committed row with a short retry.
/// </summary>
public class EmailChangeTokenDataHelper : DataHelperBase
{
    public EmailChangeTokenDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<string> GetToken(Guid memberId)
    {
        const string sql =
            "SELECT ConfirmationToken FROM MemberEmailAddressUpdateTokens WHERE MemberId = @memberId";

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
            $"No pending email-change token found for member '{memberId}'. Was the change requested first?");
    }
}
