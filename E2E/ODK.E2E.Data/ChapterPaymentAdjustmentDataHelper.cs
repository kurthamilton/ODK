namespace ODK.E2E.Data;

/// <summary>
/// Seeds and reads what a group owes (<c>ChapterPaymentAdjustments</c>) and what its transfers have paid
/// down (<c>ChapterPaymentAdjustmentRecoveries</c>). A balance is per group <em>and per currency</em> -
/// amounts in different currencies are never netted against each other - so a seeded row has to carry the
/// currency the payment under test is denominated in, or the app will not net it off at all.
/// </summary>
/// <remarks>
/// Amounts are signed: negative is owed to the platform by the group, which is what a transfer collects.
/// There is no admin UI for raising one by hand, so a test seeds it directly. The rows cascade away with
/// the chapter, so <c>TestDataCleaner</c> needs nothing for them.
/// </remarks>
public class ChapterPaymentAdjustmentDataHelper : DataHelperBase
{
    /// <summary>
    /// <c>ChapterPaymentAdjustmentType.Manual</c>: raised by a site admin, for anything the other types do
    /// not describe. What a seeded debt is, since no refund produced it.
    /// </summary>
    public const int ManualTypeId = 2;

    public ChapterPaymentAdjustmentDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Records an amount owed between the platform and the group, returning its id. Pass a negative
    /// <paramref name="amount"/> for a debt the group's next transfer should collect.
    /// </summary>
    public async Task<Guid> Create(
        Guid chapterId,
        Guid currencyId,
        decimal amount,
        string description,
        int typeId = ManualTypeId)
    {
        const string sql =
            """
            DECLARE @id UNIQUEIDENTIFIER = NEWID();

            INSERT INTO ChapterPaymentAdjustments
                (Id, ChapterId, CurrencyId, Amount, RecoveredAmount, Description,
                 ChapterPaymentAdjustmentTypeId, CreatedUtc)
            VALUES (@id, @chapterId, @currencyId, @amount, 0, @description, @typeId, @now);

            SELECT @id;
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@currencyId", currencyId)
            .AddParameter("@amount", amount)
            .AddParameter("@description", description)
            .AddParameter("@typeId", typeId)
            .AddParameter("@now", DateTime.UtcNow);

        return await builder.ExecuteScalar<Guid>();
    }

    /// <summary>
    /// How much of the adjustment has been paid down, signed as its amount is, or null if it is absent.
    /// </summary>
    public async Task<decimal?> GetRecoveredAmount(Guid adjustmentId)
    {
        const string sql = "SELECT RecoveredAmount FROM ChapterPaymentAdjustments WHERE Id = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", adjustmentId);

        return await builder.ExecuteScalar<decimal?>();
    }

    /// <summary>
    /// What the given payment's transfer paid off this adjustment, or null where it paid off none of it.
    /// This is what explains a transfer smaller than the payment behind it says.
    /// </summary>
    public async Task<decimal?> GetRecoveryAmount(Guid adjustmentId, Guid paymentId)
    {
        const string sql =
            """
            SELECT Amount
            FROM ChapterPaymentAdjustmentRecoveries
            WHERE ChapterPaymentAdjustmentId = @adjustmentId AND PaymentId = @paymentId
            """;

        await using var builder = Builder(sql)
            .AddParameter("@adjustmentId", adjustmentId)
            .AddParameter("@paymentId", paymentId);

        return await builder.ExecuteScalar<decimal?>();
    }
}
