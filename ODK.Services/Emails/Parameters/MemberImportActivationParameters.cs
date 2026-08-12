namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to an imported member who has no account yet, so they can activate one.
/// </summary>
public sealed class MemberImportActivationParameters : EmailTypeParameters
{
    private const string UrlName = "account.urls.activate";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
    }
}
