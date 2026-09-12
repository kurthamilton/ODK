using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class InvitedMembersAdminPageViewModel
{
    /// <summary>
    /// Whether sending is offered: the group holds invites it has not emailed, and it is published, so the
    /// link an invite carries lands somewhere. Who may send them is the page's securable, not this.
    /// </summary>
    public bool CanSend => Held > 0 && Chapter.IsPublished();

    public required Chapter Chapter { get; init; }

    /// <summary>How many of the invites have yet to be emailed.</summary>
    public int Held => Invited.Count(x => x.SentUtc == null);

    public required IReadOnlyCollection<InvitedMemberViewModel> Invited { get; init; }

    public required PlatformType Platform { get; init; }

    public required int RetentionDays { get; init; }
}
