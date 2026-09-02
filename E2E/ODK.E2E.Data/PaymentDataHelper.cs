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
    /// What the member's payment to the chapter records about transferring the group's share, or null if no
    /// payment has been recorded yet. Every value is null until the settlement has been read.
    /// </summary>
    public async Task<TestPaymentTransfer?> GetTransfer(Guid memberId, Guid chapterId)
    {
        const string sql =
            """
            SELECT ActualConnectedAccountAmount, ExternalTransferId, TransferWithheldAmount, TransferredUtc
            FROM Payments
            WHERE MemberId = @memberId AND ChapterId = @chapterId
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@chapterId", chapterId);

        var rows = await builder.ReadMany(reader => new TestPaymentTransfer(
            reader.IsDBNull(0) ? null : reader.GetDecimal(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2),
            reader.IsDBNull(3) ? null : reader.GetDateTime(3)));

        return rows.SingleOrDefault();
    }
}
