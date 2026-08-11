namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent when someone signs up with an address that already has an account. The sign-up page cannot
/// say so without confirming the address exists, so this email is how they find out.
/// </summary>
public sealed class DuplicateEmailParameters : EmailTypeParameters
{
    private const string LegacyLoginUrlName = "loginUrl";

    private const string LoginUrlName = "account.urls.login";

    public static IReadOnlyCollection<string> Names { get; } = [LoginUrlName];

    public string? LoginUrl { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, LoginUrlName, LoginUrl);
        Add(values, LegacyLoginUrlName, LoginUrl);
    }
}
