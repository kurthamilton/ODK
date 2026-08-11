namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent when a member asks to reset their password.
/// </summary>
public sealed class PasswordResetParameters : EmailTypeParameters
{
    private const string UrlName = "account.urls.passwordReset";

    public static IReadOnlyCollection<string> Names { get; } = [UrlName];

    public string? Url { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, UrlName, Url);
    }
}
