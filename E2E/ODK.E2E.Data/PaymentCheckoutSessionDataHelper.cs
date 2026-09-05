namespace ODK.E2E.Data;

/// <summary>
/// Writes and reads the <c>PaymentCheckoutSessions</c> row a checkout leaves behind. A test seeds one where
/// it needs the state a real checkout reaches without driving checkout - a recurring subscription that can
/// be put on a Stripe test clock cannot be created through the app's own checkout, so the rows checkout
/// would have written are arranged here instead.
/// </summary>
public class PaymentCheckoutSessionDataHelper : DataHelperBase
{
    public PaymentCheckoutSessionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Writes an open checkout session against the payment - started, not yet completed, which is where a
    /// checkout leaves it until the completion webhook arrives. Returns the new row's id.
    /// </summary>
    public async Task<Guid> Add(Guid memberId, Guid paymentId, string sessionId)
    {
        const string sql =
            """
            INSERT INTO PaymentCheckoutSessions (Id, MemberId, PaymentId, SessionId, StartedUtc)
            OUTPUT inserted.Id
            VALUES (NEWID(), @memberId, @paymentId, @sessionId, GETUTCDATE())
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@paymentId", paymentId)
            .AddParameter("@sessionId", sessionId);

        return await builder.ExecuteScalar<Guid>();
    }

    /// <summary>
    /// When the session was marked completed, or null while it is still open. The completion webhook is what
    /// closes it, so a test polls this to see the webhook has been processed.
    /// </summary>
    public async Task<DateTime?> GetCompletedUtc(Guid id)
    {
        const string sql = "SELECT CompletedUtc FROM PaymentCheckoutSessions WHERE Id = @id";

        await using var builder = Builder(sql).AddParameter("@id", id);
        return await builder.ExecuteScalar<DateTime?>();
    }
}
