namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to a member's new address to confirm they own it before the change takes effect.
/// </summary>
public sealed class EmailAddressUpdateParameters : EmailTypeParameters
{
    private const string UrlName = "account.urls.confirmEmailAddressUpdate";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
    }
}
