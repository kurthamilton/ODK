using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows;

/// <summary>
/// Everything the account machine's guards, its state resolver and its steps read, loaded in one go before
/// any of them run. A guard takes no dependencies and issues no query, so anything that needs one is
/// resolved here first.
/// </summary>
/// <remarks>
/// <para>
/// Scoped to one member and one group: <see cref="Invite"/> is the invitation for <see cref="ChapterId"/>
/// and membership is read from <see cref="Member"/> for that same group.
/// </para>
/// <para>
/// This is the union of what every transition needs, and only the Join transitions carry steps so far. The
/// sign-up transitions load most of the same rows, so the union is expected to stay close to this shape -
/// but if it starts collecting members that only one trigger can populate, that is the signal to split the
/// machine rather than to make them optional.
/// </para>
/// </remarks>
public sealed class AccountContext
{
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    /// <summary>
    /// Whether the group puts new members in front of an admin. Resolved here because it depends on the
    /// group's membership settings, on the owner's subscription features, and on whether the member holds
    /// an invitation - an invitation is approval, since the group asked them in.
    /// </summary>
    public required bool ApprovalRequired { get; init; }

    public required Guid ChapterId { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    /// <summary>The invitation the group has outstanding for this member, where there is one.</summary>
    public MemberChapterInvite? Invite { get; init; }

    /// <summary>
    /// The invitation token a sign-up presented. Trusted only against the account the submitted address
    /// resolves to, since a token posted with any other address proves nothing about it.
    /// </summary>
    public string? InviteToken { get; init; }

    /// <summary>Null when no account exists for the address yet.</summary>
    public Member? Member { get; init; }

    /// <summary>How many members the group already has, which its subscription caps.</summary>
    public required int MemberCount { get; init; }

    /// <summary>The submitted answers as domain rows, ready to write and to put in the admin email.</summary>
    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }

    public ChapterMembershipSettings? MembershipSettings { get; init; }

    public required IReadOnlyCollection<MemberNotificationSettings> NotificationSettings { get; init; }

    /// <summary>The group owner's site subscription, which decides the group's member limit.</summary>
    public SiteSubscription? OwnerSubscription { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionFeature> OwnerSubscriptionFeatures { get; init; }

    public required PlatformType Platform { get; init; }

    /// <summary>The submitted answers as posted, which is what validation reports against.</summary>
    public required IReadOnlyCollection<MemberPropertyUpdateModel> Properties { get; init; }

    /// <summary>
    /// The account the transition acts on. A step only ever runs on a transition out of a state that has
    /// one, so its absence is a fault in the definition rather than anything a member did.
    /// </summary>
    public Member RequiredMember => Member ?? throw new InvalidOperationException(
        "The transition is acting on an account that does not exist");

    public required IChapterServiceRequest Request { get; init; }

    /// <summary>Whether an OAuth provider confirms the address being registered belongs to the signer-up.</summary>
    public required bool VerifiedByOAuth { get; init; }
}
