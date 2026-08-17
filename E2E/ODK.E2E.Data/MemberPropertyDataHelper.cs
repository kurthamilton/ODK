namespace ODK.E2E.Data;

/// <summary>
/// Reads a member's answers to chapter properties (<c>MemberProperties</c>) straight from the database,
/// to assert that join/profile-update form answers were persisted.
/// </summary>
public class MemberPropertyDataHelper : DataHelperBase
{
    public MemberPropertyDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>The member's answer to a chapter property, or null if they haven't answered it.</summary>
    public async Task<string?> GetValue(string emailAddress, Guid chapterPropertyId)
    {
        const string sql =
            """
            SELECT mp.Value
            FROM MemberProperties mp
            INNER JOIN Members m ON m.Id = mp.MemberId
            WHERE m.EmailAddress = @email AND mp.ChapterPropertyId = @propertyId
            """;

        for (var attempt = 0; attempt < 10; attempt++)
        {
            await using var builder = Builder(sql)
                .AddParameter("@email", emailAddress)
                .AddParameter("@propertyId", chapterPropertyId);

            var value = await builder.ExecuteScalar<string>();
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            await Task.Delay(250);
        }

        return null;
    }
}
