namespace ODK.E2E.Data;

/// <summary>
/// Reads member records directly from the database - e.g. to resolve a member's id for the member-facing
/// profile page URL (<c>/.../members/{memberId}</c>).
/// </summary>
public class MemberDataHelper : DataHelperBase
{
    public MemberDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<Guid> GetMemberId(string emailAddress)
    {
        const string sql = "SELECT MemberId FROM Members WHERE EmailAddress = @email";

        await using var builder = Builder(sql)
            .AddParameter("@email", emailAddress);

        var memberId = await builder.ExecuteScalar<Guid?>();
        return memberId
            ?? throw new InvalidOperationException($"No member found with email '{emailAddress}'.");
    }
}
