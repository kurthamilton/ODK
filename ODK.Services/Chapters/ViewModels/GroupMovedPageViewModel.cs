using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// The signpost a group leaves on the platform it came from. It answers three questions and no others:
/// is this my group, where has it gone, and how do I carry on being involved.
/// </summary>
public class GroupMovedPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The group's own settings for its contact page, where it has changed any. Null means the defaults,
    /// which is a visible page - a group only has a row once it has said something about one.
    /// </summary>
    public required ChapterPage? ContactPage { get; init; }

    public required ChapterImageVersionDto? Image { get; init; }

    /// <summary>
    /// The token an accept-invite link carries, present only where <see cref="Visitor"/> is
    /// <see cref="GroupMovedVisitorState.Invited"/>.
    /// </summary>
    public string? InviteToken { get; init; }

    public required ChapterMigration Migration { get; init; }

    /// <summary>
    /// The group's next published event, where it has one. A member arriving from the old platform sees
    /// somewhere to be rather than only somewhere to sign up.
    /// </summary>
    public required GroupPageListEventViewModel? NextEvent { get; init; }

    public required string? ShortDescription { get; init; }

    /// <summary>
    /// Whether to offer a way of asking the organisers a question. Reads a missing page row as visible,
    /// the way the group menu does.
    /// </summary>
    public bool ShowContact => ContactPage?.Hidden != true;

    /// <summary>Who is looking, which decides what the page offers them.</summary>
    public required GroupMovedVisitorState Visitor { get; init; }
}
