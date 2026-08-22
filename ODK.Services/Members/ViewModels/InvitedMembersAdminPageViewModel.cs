using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class InvitedMembersAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Everyone the group has asked to join who has yet to accept, oldest invitation first. Ordered by the
    /// query rather than the view: the order is what the page is specified to show, so it belongs with the
    /// read that establishes it.
    /// </summary>
    public required IReadOnlyCollection<MemberChapterInviteDto> Invited { get; init; }

    public required PlatformType Platform { get; init; }
}
