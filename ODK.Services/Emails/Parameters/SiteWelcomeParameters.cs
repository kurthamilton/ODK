using ODK.Core.Members;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Welcomes a member to the site once their account is active.
/// </summary>
public sealed class SiteWelcomeParameters : EmailTypeParameters
{
    private const string FirstNameName = "member.firstName";

    private const string GroupsUrlName = "account.urls.groups";

    private readonly Member _member;

    public SiteWelcomeParameters(Member member)
    {
        _member = member;
    }

    public static IReadOnlyCollection<string> Names { get; } = [FirstNameName, GroupsUrlName];

    public required string GroupsUrl { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, FirstNameName, _member.FirstName);
        Add(values, GroupsUrlName, GroupsUrl);
    }
}
