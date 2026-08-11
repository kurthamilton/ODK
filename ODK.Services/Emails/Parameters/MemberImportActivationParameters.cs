namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to an imported member who has no account yet, so they can activate one.
/// </summary>
public sealed class MemberImportActivationParameters : EmailTypeParameters
{
    private const string UrlName = "group.urls.activateAccount";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
        Add(values, LegacyUrlName, Url);
    }
}
