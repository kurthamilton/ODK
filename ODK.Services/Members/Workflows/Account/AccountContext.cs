using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Referrals;
using ODK.Core.Workflows;
using ODK.Core.Topics;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

/// <summary>
/// Everything the account machine's guards, its state resolver and its steps read, loaded in one go before
/// any of them run. A guard takes no dependencies and issues no query, so anything that needs one is resolved
/// here first - but only cheap reads. Anything expensive or outbound is left to the step that needs it, so a
/// transition that does not run that step does not pay for it.
/// </summary>
/// <remarks>
/// Anything one step produces and another needs is resolved by the factory and arrives here as an input - the
/// activation token most notably, which the caller also needs back, so it is an input rather than something a
/// step invents. <see cref="NewMember"/> is the single exception, and says there why.
/// </remarks>
public sealed class AccountContext
{
    private readonly WriteOnce<Member> _newMember = new("The account the transition creates");

    /// <summary>
    /// The invitation an accept-invitation link named, resolved from the token it carried. Distinct from
    /// <see cref="Invite"/>, which is one of the invitations read off an account a sign-up is discarding.
    /// </summary>
    public MemberChapterInvite? AcceptedInvite { get; init; }

    /// <summary>
    /// The token the account will be activated with. Minted by the factory, or carried over from an
    /// unactivated account being discarded and recreated so a link already emailed still works.
    /// </summary>
    public string? ActivationToken { get; init; }

    /// <summary>
    /// The group's admins, who hear about a new member when their account is activated - not when it is
    /// created, since an account that never activates is nobody the group needs telling about.
    /// </summary>
    public IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; } = [];

    /// <summary>
    /// Invitations held by an unactivated account that a sign-up is about to discard and recreate. They are
    /// read before the delete cascades them away, and re-raised against the new account.
    /// </summary>
    public IReadOnlyCollection<MemberChapterInvite> CarriedOverInvites { get; init; } = [];

    /// <summary>
    /// Where the sign-up happened, when it happened inside a group. An account is site-level, so this is
    /// provenance rather than state: on Drunken Knitwits signing up to a group creates the account, which
    /// scopes its activation token and takes the group's timezone. Null for a sign-up to the site itself.
    /// </summary>
    public Chapter? Chapter { get; init; }

    public ChapterLocation? ChapterLocation { get; init; }

    /// <summary>The group's country, which an imported member inherits along with its location.</summary>
    public Country? Country { get; init; }

    /// <summary>The group's currency, which an imported member is billed in.</summary>
    public Currency? Currency { get; init; }

    public IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; } = [];

    /// <summary>
    /// Whether the sign-up presented the token from an invitation sent to the address being registered.
    /// Holding it proves the sign-up reached that inbox, which is everything an activation email establishes.
    /// Derived here so the guard that picks the edge and the caller that reports the outcome read one rule.
    /// </summary>
    public bool PresentedTheInviteToken => Invite != null &&
        !string.IsNullOrEmpty(Profile?.InviteToken) &&
        Invite.Token == Profile.InviteToken;

    /// <summary>The invitation this group has outstanding for the address, where the sign-up carries one.</summary>
    public MemberChapterInvite? Invite => CarriedOverInvites
        .FirstOrDefault(x => Chapter != null && x.ChapterId == Chapter.Id);

    /// <summary>The row an admin imported. Null for any trigger other than an import.</summary>
    public MemberImportModel? Import { get; init; }

    /// <summary>What the accept-invitation form submitted. Null for any other trigger.</summary>
    public InvitationAcceptModel? Invitation { get; init; }

    /// <summary>Null when no account exists for the address yet.</summary>
    public Member? Member { get; init; }

    /// <summary>How many members the group already has, which its owner's subscription caps.</summary>
    public int MemberCount { get; init; }

    /// <summary>The member's stored password, where they already have one. Null until they set the first.</summary>
    public MemberPassword? MemberPassword { get; init; }

    /// <summary>
    /// The member's answers to the group's questions, which the email to its admins reads. Empty for any
    /// transition that does not tell a group about somebody.
    /// </summary>
    public IReadOnlyCollection<MemberProperty> MemberProperties { get; init; } = [];

    public ChapterMembershipSettings? MembershipSettings { get; init; }

    /// <summary>
    /// The account a sign-up creates, set by the step that adds it so the steps after it can write the rows
    /// that hang off it.
    /// </summary>
    /// <remarks>
    /// The one value a transition's steps pass between themselves, and the only writable member here.
    /// Everything else a step needs is resolved by the factory and arrives as an input - but this member does
    /// not exist when the context is built, and its key is assigned when it is added, so it cannot be one.
    /// A <see cref="WriteOnce{T}"/> slot: no transition creates two accounts, so a second write means a
    /// definition put two create steps on one edge, and failing there beats silently keeping whichever ran
    /// last.
    /// </remarks>
    public Member? NewMember
    {
        get => _newMember.Value;
        set => _newMember.Value = value;
    }

    /// <summary>The group owner's site subscription, which decides the group's member limit.</summary>
    public SiteSubscription? OwnerSubscription { get; init; }

    /// <summary>The password an activation submitted, before it has been validated or hashed.</summary>
    public string? NewPassword { get; init; }

    /// <summary>Which of the group's admins have asked to hear about new members.</summary>
    public IReadOnlyCollection<MemberNotificationSettings> NotificationSettings { get; init; } = [];

    public IReadOnlyCollection<SiteSubscriptionFeature> OwnerSubscriptionFeatures { get; init; } = [];

    /// <summary>
    /// The activation token row being spent, on the transition that activates. Distinct from
    /// <see cref="ActivationToken"/>, which is the string a sign-up issues - this is the record an
    /// activation consumes and deletes.
    /// </summary>
    public MemberActivationToken? PendingActivation { get; init; }

    /// <summary>
    /// What a group sign-up submitted. Null for any other trigger, including a sign-up to the site, which
    /// submits a different form - see <see cref="SiteProfile"/>.
    /// </summary>
    public MemberCreateProfile? Profile { get; init; }

    /// <summary>What a sign-up to the site submitted. Null for any other trigger.</summary>
    public AccountCreateModel? SiteProfile { get; init; }

    /// <summary>
    /// The address the sign-up registered, whichever form it came from. The two share almost nothing else,
    /// but every sign-up has an address and the steps that only need that should not care which form ran.
    /// </summary>
    public string SignUpEmailAddress => Profile?.EmailAddress
        ?? SiteProfile?.EmailAddress
        ?? throw new InvalidOperationException("The transition is acting on a sign-up that submitted nothing");

    /// <summary>
    /// The account the transition acts on. A step only ever runs on a transition out of a state that has
    /// one, so its absence is a fault in the definition rather than anything a member did.
    /// </summary>
    public Member RequiredMember => Member ?? throw new InvalidOperationException(
        "The transition is acting on an account that does not exist");

    /// <summary>
    /// The account the transition is about, whether it has just created one or found the one it acts on. A
    /// step that only needs "the member this is happening to" - welcoming them, emailing them - should not
    /// have to know which of the two got it here.
    /// </summary>
    public Member RequiredAccount => NewMember ?? Member ?? throw new InvalidOperationException(
        "The transition names no account");

    /// <summary>The invitation being accepted, on a transition only an acceptance can reach.</summary>
    public MemberChapterInvite RequiredAcceptedInvite => AcceptedInvite ?? throw new InvalidOperationException(
        "The transition accepts an invitation but none was resolved");

    /// <summary>What the accept-invitation form submitted, on a transition only an acceptance can reach.</summary>
    public InvitationAcceptModel RequiredInvitation => Invitation ?? throw new InvalidOperationException(
        "The transition is acting on an acceptance that submitted nothing");

    /// <summary>The password an activation submitted, on a transition that sets one.</summary>
    public string RequiredNewPassword => NewPassword ?? throw new InvalidOperationException(
        "The transition sets a password but none was submitted");

    /// <summary>The activation row being spent, on the transition that activates.</summary>
    public MemberActivationToken RequiredPendingActivation => PendingActivation
        ?? throw new InvalidOperationException("The transition activates but no activation record was resolved");

    /// <summary>The group the sign-up is joining, on a transition that only a group sign-up can reach.</summary>
    public Chapter RequiredChapter => Chapter ?? throw new InvalidOperationException(
        "The transition is acting on a group sign-up that names no group");

    /// <summary>The account the sign-up created, on a step that runs after the one that adds it.</summary>
    public Member RequiredNewMember => NewMember ?? throw new InvalidOperationException(
        "The step runs before the one that creates the account");

    /// <summary>The token this sign-up will be activated with, on a transition that issues one.</summary>
    public string RequiredActivationToken => ActivationToken ?? throw new InvalidOperationException(
        "The transition issues an activation token but none was resolved");

    /// <summary>The platform's default site subscription, on a transition that creates an account.</summary>
    public SiteSubscription RequiredSiteSubscription => SiteSubscription ?? throw new InvalidOperationException(
        "The transition creates an account but no default site subscription was resolved");

    /// <summary>What a group sign-up submitted, on a transition only a group sign-up can reach.</summary>
    public MemberCreateProfile RequiredProfile => Profile ?? throw new InvalidOperationException(
        "The transition is acting on a group sign-up that submitted nothing");

    /// <summary>The imported row, on a transition only an import can reach.</summary>
    public MemberImportModel RequiredImport => Import ?? throw new InvalidOperationException(
        "The transition is acting on an import that names no member");

    /// <summary>What a site sign-up submitted, on a transition only a site sign-up can reach.</summary>
    public AccountCreateModel RequiredSiteProfile => SiteProfile ?? throw new InvalidOperationException(
        "The transition is acting on a site sign-up that submitted nothing");

    /// <summary>
    /// The referral the sign-up arrived through, resolved from the id the browser posted. Null when there was
    /// none, or when the posted id matched nothing.
    /// </summary>
    public Referral? Referral { get; init; }

    /// <summary>The topics the sign-up chose, where its form offers them.</summary>
    public IReadOnlyCollection<Topic> Topics { get; init; } = [];

    public required IServiceRequest Request { get; init; }

    public SiteSubscription? SiteSubscription { get; init; }

    /// <summary>Whether an OAuth provider confirms the address being registered belongs to the signer-up.</summary>
    public required bool VerifiedByOAuth { get; init; }
}
