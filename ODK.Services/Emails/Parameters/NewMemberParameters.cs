namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Welcomes a member who has just joined a group.
/// </summary>
public sealed class NewMemberParameters : EmailTypeParameters
{
    private const string EventsUrlName = "group.urls.events";

    private const string FirstNameName = "member.firstName";

    public static IReadOnlyCollection<string> Names { get; } = [EventsUrlName, FirstNameName];

    public required string EventsUrl { get; init; }

    public required string FirstName { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, EventsUrlName, EventsUrl);
        Add(values, FirstNameName, FirstName);
    }
}
