namespace ODK.Core.Members;

/// <summary>
/// An outstanding invitation for a member to join a chapter, written by the member import.
/// </summary>
/// <remarks>
/// <para>
/// Separate from <see cref="MemberChapter"/> rather than a flag on it, because an invitation is not a
/// membership: an imported member has no membership status until they have activated their account and joined.
/// Keeping the two apart means every existing "is a member of" read stays correct without being audited - a
/// flag on the membership row would grant membership everywhere that forgot to check it.
/// </para>
/// <para>
/// The <see cref="Token"/> is what makes the invitation usable by someone who cannot sign in yet, which on
/// Drunken Knitwits is everyone it is sent to: an imported member has no password until they set one. Holding it
/// proves they received the email at the address the import supplied, so a sign-up arriving with a valid token
/// needs no separate activation email to prove the same thing.
/// </para>
/// <para>
/// That makes the address in the imported file the trust anchor. A mistyped address there produces an account
/// that whoever received the link can activate, so the import preview is where a wrong address has to be caught.
/// </para>
/// <para>
/// The invitation is consumed when it is accepted, the way a <see cref="MemberActivationToken"/> is - so its
/// absence is the record that the member joined, and <see cref="MemberChapter"/> is the record of when.
/// </para>
/// </remarks>
public class MemberChapterInvite : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    /// <summary>
    /// Emailed to the member as part of the invitation link. See the remarks on the type.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
