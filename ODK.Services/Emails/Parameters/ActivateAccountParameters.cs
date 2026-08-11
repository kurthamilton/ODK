namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to someone who has signed up and needs to activate their account.
/// </summary>
public sealed class ActivateAccountParameters : EmailTypeParameters
{
    private const string UrlName = "account.urls.activate";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
    }
}
