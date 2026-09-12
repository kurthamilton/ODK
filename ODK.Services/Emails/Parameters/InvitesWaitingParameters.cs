using System.Globalization;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a group's owner that invites an import raised are waiting to be emailed, and where to send them.
/// </summary>
public sealed class InvitesWaitingParameters : EmailTypeParameters
{
    private const string CountName = "invites.count";

    private const string UrlName = "invites.url";

    private readonly CultureInfo _culture;

    public InvitesWaitingParameters(CultureInfo culture)
    {
        _culture = culture;
    }

    public static IReadOnlyCollection<string> Names { get; } = [CountName, UrlName];

    public required int Count { get; init; }

    public required string Url { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, CountName, Count.ToString(_culture));
        Add(values, UrlName, Url);
    }
}
