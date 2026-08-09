namespace ODK.E2E.Data;

/// <summary>
/// Reads site FAQ state directly from the database to assert admin outcomes. Site questions are scoped
/// to a platform rather than to a member or chapter, so nothing cascades them away - test rows carry
/// <see cref="TestNamePrefix"/> so <see cref="TestDataCleaner"/> can find and remove them.
/// </summary>
public class SiteQuestionDataHelper : DataHelperBase
{
    public const string TestNamePrefix = "e2e-faq-";

    public SiteQuestionDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<int?> GetDisplayOrder(string name)
    {
        const string sql =
            """
            SELECT TOP 1 DisplayOrder
            FROM SiteQuestions
            WHERE Name = @name
            """;

        await using var builder = Builder(sql).AddParameter("@name", name);

        return await builder.ExecuteScalar<int?>();
    }

    public async Task<Guid?> GetQuestionId(string name)
    {
        const string sql =
            """
            SELECT TOP 1 SiteQuestionId
            FROM SiteQuestions
            WHERE Name = @name
            """;

        await using var builder = Builder(sql).AddParameter("@name", name);

        return await builder.ExecuteScalar<Guid?>();
    }

    /// <summary>
    /// The platform the question was filed under, as the stored int. Questions are per-platform, so this
    /// is what proves a question created on one platform didn't land on the other.
    /// </summary>
    public async Task<int?> GetPlatform(string name)
    {
        const string sql =
            """
            SELECT TOP 1 PlatformTypeId
            FROM SiteQuestions
            WHERE Name = @name
            """;

        await using var builder = Builder(sql).AddParameter("@name", name);

        return await builder.ExecuteScalar<int?>();
    }

    public async Task<bool> QuestionExists(string name) => await GetQuestionId(name) is not null;
}
