using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Reads what the app recorded about a payment (<c>Payments</c>) once it settled: the group's share, the
/// provider's transfer of it, and how much was withheld against what the group owed. None of it is visible
/// to a user, and all of it is written asynchronously - the settlement is read on a scheduled job after the
/// purchase webhook - so tests poll these.
/// </summary>
public class PaymentDataHelper : DataHelperBase
{
    public PaymentDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Writes the <c>Payments</c> row a site-subscription checkout leaves behind before the member has paid:
    /// unpaid, priced from the subscription's own price, and named by the same reference the app builds. The
    /// completion webhook is what marks it paid, so a test seeding this is arranging the state a real
    /// purchase reaches by the time Stripe first charges. Returns null if the price does not exist.
    /// </summary>
    public async Task<Guid?> AddUnpaidSiteSubscriptionPayment(
        Guid memberId, Guid siteSubscriptionPriceId, int environmentTypeId, int platformTypeId)
    {
        const string sql =
            """
            INSERT INTO Payments
                (Id, MemberId, CurrencyId, Amount, Reference, CreatedUtc,
                 EnvironmentTypeId, PaymentProviderTypeId, PlatformTypeId)
            OUTPUT inserted.Id
            SELECT NEWID(), @memberId, price.CurrencyId, price.Amount,
                   'Subscription: ' + sub.Name, GETUTCDATE(),
                   @environmentTypeId, @paymentProviderTypeId, @platformTypeId
            FROM SiteSubscriptionPrices price
            INNER JOIN SiteSubscriptions sub ON sub.Id = price.SiteSubscriptionId
            WHERE price.Id = @priceId
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@priceId", siteSubscriptionPriceId)
            .AddParameter("@environmentTypeId", environmentTypeId)
            .AddParameter("@paymentProviderTypeId", PaymentProviderTypeIds.Stripe)
            .AddParameter("@platformTypeId", platformTypeId);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// How many payments the member has. A billing event that took money should leave one, so this is what
    /// separates a renewal recording its own charge from a renewal recorded against the first purchase's.
    /// </summary>
    public async Task<int> GetCount(Guid memberId)
    {
        const string sql = "SELECT COUNT(1) FROM Payments WHERE MemberId = @memberId";

        await using var builder = Builder(sql).AddParameter("@memberId", memberId);
        return await builder.ExecuteScalar<int>();
    }

    /// <summary>The id of the member's payment to the chapter, or null if none has been recorded yet.</summary>
    public async Task<Guid?> GetId(Guid memberId, Guid chapterId)
    {
        const string sql =
            "SELECT Id FROM Payments WHERE MemberId = @memberId AND ChapterId = @chapterId";

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// When the payment was marked paid, or null while it is still awaiting the completion webhook. Only a
    /// payment the test knows the id of, so it says what became of one particular row rather than of
    /// whichever row a member happens to have.
    /// </summary>
    public async Task<DateTime?> GetPaidUtc(Guid paymentId)
    {
        const string sql = "SELECT PaidUtc FROM Payments WHERE Id = @paymentId";

        await using var builder = Builder(sql).AddParameter("@paymentId", paymentId);
        return await builder.ExecuteScalar<DateTime?>();
    }

    /// <summary>
    /// What the member's payment to the chapter records about transferring the group's share, or null if
    /// the settlement has not been read yet - the share is not worked out, and no row written, until it
    /// has.
    /// </summary>
    public async Task<TestPaymentTransfer?> GetTransfer(Guid memberId, Guid chapterId)
    {
        const string sql =
            """
            SELECT t.Amount, t.ExternalId, t.WithheldAmount, t.CompletedUtc
            FROM PaymentTransfers t
            INNER JOIN Payments p ON p.Id = t.PaymentId
            WHERE p.MemberId = @memberId AND p.ChapterId = @chapterId
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        var rows = await builder.ReadMany(reader => new TestPaymentTransfer(
            reader.GetDecimal(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2),
            reader.IsDBNull(3) ? null : reader.GetDateTime(3)));

        return rows.SingleOrDefault();
    }

    /// <summary>
    /// The same, for one payment named by id. A member who has been billed more than once has a payment and
    /// a transfer per billing, so a test about one of them has to say which - <see cref="GetTransfer"/>
    /// would find both and answer about neither.
    /// </summary>
    public async Task<TestPaymentTransfer?> GetTransferForPayment(Guid paymentId)
    {
        const string sql =
            """
            SELECT t.Amount, t.ExternalId, t.WithheldAmount, t.CompletedUtc
            FROM PaymentTransfers t
            WHERE t.PaymentId = @paymentId
            """;

        await using var builder = Builder(sql).AddParameter("@paymentId", paymentId);

        var rows = await builder.ReadMany(reader => new TestPaymentTransfer(
            reader.GetDecimal(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2),
            reader.IsDBNull(3) ? null : reader.GetDateTime(3)));

        return rows.SingleOrDefault();
    }
}
