namespace ODK.E2E.Data;

/// <summary>
/// Reads event state directly from the database to assert admin outcomes (an event was created for a
/// chapter) and to drive the member-facing flows that need an event's id or shortcode.
/// </summary>
public class EventDataHelper : DataHelperBase
{
    public EventDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<Guid?> GetEventId(Guid chapterId, string name)
    {
        const string sql =
            """
            SELECT TOP 1 EventId
            FROM Events
            WHERE ChapterId = @chapterId AND Name = @name
            ORDER BY CreatedUtc DESC
            """;

        await using var builder = Builder(sql)
            .AddParameter("@chapterId", chapterId)
            .AddParameter("@name", name);

        return await builder.ExecuteScalar<Guid?>();
    }

    public async Task<string?> GetName(Guid eventId)
    {
        const string sql = "SELECT Name FROM Events WHERE EventId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", eventId);

        return await builder.ExecuteScalar<string>();
    }

    public async Task<string> GetShortcode(Guid eventId)
    {
        const string sql = "SELECT Shortcode FROM Events WHERE EventId = @id";

        await using var builder = Builder(sql)
            .AddParameter("@id", eventId);

        return await builder.ExecuteScalar<string>()
            ?? throw new InvalidOperationException($"No event found with id '{eventId}'.");
    }
}
