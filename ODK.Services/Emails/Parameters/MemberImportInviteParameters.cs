namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to an imported member who already has an account, telling them they have been added.
/// </summary>
public sealed class MemberImportInviteParameters : EmailTypeParameters
{
    private const string UrlName = "group.urls.join";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
    }
}
