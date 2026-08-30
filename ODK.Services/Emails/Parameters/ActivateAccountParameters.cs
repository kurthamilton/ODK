namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Sent to someone who has signed up and needs to activate their account.
/// </summary>
public sealed class ActivateAccountParameters : EmailTypeParameters
{
    private const string ActivateAccountUrlName = "account.urls.activate";

    public static IReadOnlyCollection<string> Names { get; } = [ActivateAccountUrlName];

    public required string ActivateAccountUrl { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, ActivateAccountUrlName, ActivateAccountUrl);
    }
}
