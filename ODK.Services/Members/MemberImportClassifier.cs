using ODK.Core.Members;
using ODK.Services.Members.Models;

namespace ODK.Services.Members;

/// <summary>
/// What a group should do about an imported address, over the accounts and outstanding invites read once for
/// the whole file. The single definition of it, so the upload deciding what to hold, the page reporting why
/// a row is waiting, and the invite deciding what to raise cannot disagree.
/// </summary>
public sealed class MemberImportClassifier
{
    private readonly Guid _chapterId;
    private readonly IReadOnlyDictionary<string, Member> _existingByEmailAddress;
    private readonly IReadOnlySet<Guid> _invitedMemberIds;

    private MemberImportClassifier(
        Guid chapterId,
        IReadOnlyDictionary<string, Member> existingByEmailAddress,
        IReadOnlySet<Guid> invitedMemberIds)
    {
        _chapterId = chapterId;
        _existingByEmailAddress = existingByEmailAddress;
        _invitedMemberIds = invitedMemberIds;
    }

    public static MemberImportClassifier Create(
        Guid chapterId,
        IReadOnlyCollection<Member> existingMembers,
        IReadOnlyCollection<MemberChapterInvite> outstandingInvites)
        => new(
            chapterId,
            existingMembers.ToDictionary(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase),
            outstandingInvites.Select(x => x.MemberId).ToHashSet());

    public MemberImportRowStatus Classify(string emailAddress, bool validEmailAddress)
    {
        if (!validEmailAddress)
        {
            return MemberImportRowStatus.Invalid;
        }

        var existing = ExistingMember(emailAddress);
        if (existing == null)
        {
            return MemberImportRowStatus.New;
        }

        if (existing.IsMemberOf(_chapterId))
        {
            return MemberImportRowStatus.ExistingInGroup;
        }

        return _invitedMemberIds.Contains(existing.Id)
            ? MemberImportRowStatus.AlreadyInvited
            : MemberImportRowStatus.ExistingNotInGroup;
    }

    /// <summary>The account already registered against an address, where there is one.</summary>
    public Member? ExistingMember(string emailAddress)
        => _existingByEmailAddress.GetValueOrDefault(emailAddress);
}
