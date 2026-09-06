namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to an imported member, asking them to join.
/// </summary>
public sealed class MemberImportInviteParameters : EmailTypeParameters
{
    private const string RefuseUrlName = "group.urls.refuseInvite";

    private const string UrlName = "group.urls.join";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName, RefuseUrlName];

    public required string RefuseUrl { get; init; }

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, RefuseUrlName, RefuseUrl);
        Add(values, UrlName, Url);
    }
}
