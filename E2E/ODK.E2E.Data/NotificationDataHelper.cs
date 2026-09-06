using ODK.E2E.Data.Models;

namespace ODK.E2E.Data;

/// <summary>
/// Reads the notifications raised for a member from <c>Notifications</c>. A notification is committed with
/// the change it announces, so a test that has already observed that change - a renewal's new expiry - can
/// read these without polling.
/// </summary>
public class NotificationDataHelper : DataHelperBase
{
    public NotificationDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The member's notifications of the given type, oldest first.</summary>
    public async Task<IReadOnlyCollection<TestNotification>> GetByType(Guid memberId, int notificationTypeId)
    {
        const string sql =
            """
            SELECT n.Text, n.ChapterId
            FROM Notifications n
            WHERE n.MemberId = @memberId AND n.NotificationTypeId = @notificationTypeId
            ORDER BY n.CreatedUtc
            """;

        await using var builder = Builder(sql)
            .AddParameter("@memberId", memberId)
            .AddParameter("@notificationTypeId", notificationTypeId);

        return await builder.ReadMany(x => new TestNotification(
            x.GetString(0),
            x.IsDBNull(1) ? null : x.GetGuid(1)));
    }
}
