using ODK.Core.Chapters;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells site admins that a group has been created.
/// </summary>
/// <remarks>
/// The new group's name is its own parameter rather than the core group.name, which is the group an email
/// is sent <em>as</em> - this one is sent as the site, and the core value is what the email is addressed
/// from. Overriding it would put the new group's name in the From line of a site notification.
/// </remarks>
public sealed class NewGroupAdminParameters : EmailTypeParameters
{
    private const string GroupNameName = "newGroup.name";

    private const string GroupsUrlName = "siteadmin.urls.groups";

    private readonly Chapter _chapter;

    public NewGroupAdminParameters(Chapter chapter)
    {
        _chapter = chapter;
    }

    public static IReadOnlyCollection<string> Names { get; } = [GroupNameName, GroupsUrlName];

    public required string GroupsUrl { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, GroupNameName, _chapter.FullName);
        Add(values, GroupsUrlName, GroupsUrl);
    }
}
