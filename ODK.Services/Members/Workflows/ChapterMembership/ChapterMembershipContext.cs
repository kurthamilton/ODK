using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Workflows;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.ChapterMembership;

/// <summary>
/// Everything the membership machine's guards, its state resolver and its steps read, loaded in one go before
/// any of them run. A guard takes no dependencies and issues no query, so anything that needs one is resolved
/// here first.
/// </summary>
/// <remarks>Scoped to one member and one group.</remarks>
public sealed class ChapterMembershipContext
{
    private readonly WriteOnce<MemberChapterInvite> _raisedInvite = new("The invite the transition raises");

    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    /// <summary>
    /// Whether the group puts new members in front of an admin: its setting, and whether the owner's
    /// subscription carries the feature at all. Whether *this* member is queued also depends on their
    /// invite, which is a state rather than a condition - see <see cref="ApprovedOnJoining"/>.
    /// </summary>
    public required bool ApprovalRequired { get; init; }

    /// <summary>
    /// What the membership row is written with. An invite is approval - the group asked them in - so an
    /// invited member is never queued however the group is configured.
    /// </summary>
    public bool ApprovedOnJoining => Invite != null || !ApprovalRequired;

    public required Guid ChapterId { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    /// <summary>The invite the group has outstanding for this member, where there is one.</summary>
    public MemberChapterInvite? Invite { get; init; }

    public required Member Member { get; init; }

    /// <summary>How many members the group already has, which its subscription caps.</summary>
    public required int MemberCount { get; init; }

    /// <summary>The submitted answers as domain rows, ready to write and to put in the admin email.</summary>
    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }

    public ChapterMembershipSettings? MembershipSettings { get; init; }

    public required IReadOnlyCollection<MemberNotificationSettings> NotificationSettings { get; init; }

    /// <summary>The group owner's site subscription, which decides the group's member limit.</summary>
    public SiteSubscription? OwnerSubscription { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionFeature> OwnerSubscriptionFeatures { get; init; }

    /// <summary>The submitted answers as posted, which is what validation reports against.</summary>
    public required IReadOnlyCollection<MemberPropertyUpdateModel> Properties { get; init; }

    /// <summary>
    /// The invite the transition raises, set by the step that adds it so the caller can decide whether to
    /// send it. Unsent when it is written - see <see cref="MemberChapterInvite.SentUtc"/>.
    /// </summary>
    /// <remarks>
    /// The one value a transition's steps pass out of themselves, and the only writable member here. It does
    /// not exist when the context is built, so it cannot be an input. A <see cref="WriteOnce{T}"/> slot: no
    /// transition raises two invites, so a second write means a definition put two raise steps on one edge.
    /// </remarks>
    public MemberChapterInvite? RaisedInvite
    {
        get => _raisedInvite.Value;
        set => _raisedInvite.Value = value;
    }

    public required IChapterServiceRequest Request { get; init; }

    /// <summary>
    /// The membership row the transition acts on. A step only ever runs on a transition out of a state that
    /// has one, so its absence is a fault in the definition rather than anything an admin did.
    /// </summary>
    public MemberChapter RequiredMemberChapter => Member.MemberChapter(ChapterId)
        ?? throw new InvalidOperationException("The transition is acting on a member who has not joined");

    /// <summary>The invite just raised, on a transition whose caller has to send it.</summary>
    public MemberChapterInvite RequiredRaisedInvite => RaisedInvite ?? throw new InvalidOperationException(
        "The transition was expected to raise an invite but none was written");
}
