using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.Workflows;

/// <summary>
/// Everything the account machine's guards read, loaded in one go before any of them run. A guard
/// takes no dependencies and issues no query, so anything that needs one is resolved here first.
/// </summary>
public sealed class AccountContext
{
    /// <summary>
    /// Whether the group puts new members in front of an admin. Resolved here because it depends on
    /// the group's membership settings and on the owner's subscription features.
    /// </summary>
    public required bool ApprovalRequired { get; init; }

    /// <summary>The invitation the group has outstanding for this address, where there is one.</summary>
    public MemberChapterInvite? Invite { get; init; }

    /// <summary>
    /// The invitation token the sign-up presented. Trusted only against the account the submitted
    /// address resolves to, since a token posted with any other address proves nothing about it.
    /// </summary>
    public string? InviteToken { get; init; }

    public required PlatformType Platform { get; init; }

    /// <summary>Whether an OAuth provider confirms the address being registered belongs to the signer-up.</summary>
    public required bool VerifiedByOAuth { get; init; }
}
