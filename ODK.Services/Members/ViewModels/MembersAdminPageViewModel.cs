using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MembersAdminPageViewModel
{
    /// <summary>
    /// Whether the admin viewing the page holds the bulk email securable. The page is the only way in to
    /// bulk email, so an admin without it sees the members list and nothing more.
    /// </summary>
    public required bool CanSendBulkEmail { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Members who have turned off the group's event emails.
    /// </summary>
    public required IReadOnlySet<Guid> EventEmailOptOutMemberIds { get; init; }

    /// <summary>
    /// Members who have turned off the group's own emails, which is what a bulk email is. They cannot be
    /// selected as recipients - see <see cref="IMemberAdminService.SendBulkEmail"/>, which drops them
    /// whatever is posted.
    /// </summary>
    /// <remarks>
    /// Not the only reason a member cannot be emailed - an unactivated account cannot either, which the
    /// member carries itself.
    /// </remarks>
    public required IReadOnlySet<Guid> GroupEmailOptOutMemberIds { get; init; }

    /// <summary>
    /// Whether the group's subscription includes sending member emails. Separate from
    /// <see cref="CanSendBulkEmail"/>: the admin may send, but the group may not have bought it.
    /// </summary>
    public required bool HasBulkEmailFeature { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> Members { get; init; }

    public required ChapterMembershipSettings? MembershipSettings { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<MemberChapterSubscription> Subscriptions { get; init; }
}
