using ODK.Core.Chapters;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells site admins that a group has asked to be approved.
/// </summary>
/// <remarks>
/// Shares its parameter names with <see cref="NewGroupAdminParameters"/>, and for the same reason: both
/// are sent as the site rather than as the group they are about, so the group's name cannot travel as the
/// core group.name without landing in the From line.
/// </remarks>
public sealed class GroupSubmittedAdminParameters : EmailTypeParameters
{
    private const string GroupNameName = "newGroup.name";

    private const string GroupsUrlName = "siteadmin.urls.groups";

    private readonly Chapter _chapter;

    public GroupSubmittedAdminParameters(Chapter chapter)
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
