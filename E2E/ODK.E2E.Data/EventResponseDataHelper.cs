namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's RSVP (event response) straight from the database, to assert an RSVP was recorded.
/// The response is written synchronously as part of the RSVP request (before its redirect), so this
/// reads the committed row - with a short retry to absorb read-after-write timing across connections.
/// </summary>
public class EventResponseDataHelper : DataHelperBase
{
    public EventResponseDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Records a "yes" (attending) response for a member against an event, straight in the database - used
    /// to fill an event's attendee capacity in tests without driving another member through the UI.
    /// </summary>
    public async Task AddAttendee(Guid eventId, string emailAddress)
    {
        const string sql =
            """
            INSERT INTO EventResponses (EventId, MemberId, ResponseTypeId)
            SELECT @eventId, MemberId, 1
            FROM Members
            WHERE EmailAddress = @email
            """;

        await using var builder = Builder(sql)
            .AddParameter("@eventId", eventId)
            .AddParameter("@email", emailAddress);

        var affected = await builder.ExecuteNonQuery();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Could not add '{emailAddress}' as an attendee of event '{eventId}' - no matching member.");
        }
    }

    /// <summary>
    /// The <c>ResponseTypeId</c> the member has recorded against the event, or null if none. Values
    /// come from <c>EventResponseType</c> (Yes = 1, Maybe = 2, No = 3, None = 0).
    /// </summary>
    public async Task<int?> GetResponseType(Guid eventId, string emailAddress)
    {
        const string sql =
            """
            SELECT r.ResponseTypeId
            FROM EventResponses r
            INNER JOIN Members m ON m.MemberId = r.MemberId
            WHERE r.EventId = @eventId AND m.EmailAddress = @email
            """;

        for (var attempt = 0; attempt < 10; attempt++)
        {
            await using var builder = Builder(sql)
                .AddParameter("@eventId", eventId)
                .AddParameter("@email", emailAddress);

            var responseType = await builder.ExecuteScalar<int?>();
            if (responseType is not null)
            {
                return responseType;
            }

            await Task.Delay(250);
        }

        return null;
    }
}
