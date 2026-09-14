using ODK.Core.Members;
using ODK.Data.Core;

namespace ODK.Services.Members;

public class MemberInviteService : IMemberInviteService
{
    private readonly MemberInviteServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberInviteService(IUnitOfWork unitOfWork, MemberInviteServiceSettings settings)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelInvite(IChapterServiceRequest request, Guid memberId)
    {
        /* Every invite the member holds, not just this group's: the account an import raised goes when the
           last invite that would have brought it to life does, so whether this is the last one is a fact
           about the member rather than about the group. */
        var (invite, member, held) = await _unitOfWork.Run(
            x => x.MemberChapterInviteRepository.GetByMemberId(memberId, request.Chapter.Id),
            x => x.MemberRepository.GetByIdOrDefault(memberId),
            x => x.MemberChapterInviteRepository.GetByMemberIds([memberId]));

        // The member is read without assuming they exist, so an id naming nobody is the same answer as an
        // id naming somebody with no invite rather than a failure to load.
        if (invite == null || member == null)
        {
            return ServiceResult.Failure("There is no outstanding invite for that person");
        }

        DiscardInvites([invite], [member], held);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful("Invite cancelled");
    }

    public async Task<int> PurgeExpiredInvites()
    {
        /* Measured from when the invite was raised, not from when it was emailed: the retention period the
           privacy policy states runs from the point the data was taken, so it cannot depend on something the
           group controls the timing of. An invite a group holds unsent would otherwise be held indefinitely. */
        var createdBeforeUtc = DateTime.UtcNow.AddDays(-_settings.RetentionDays);

        var expired = await _unitOfWork.MemberChapterInviteRepository
            .GetCreatedBefore(createdBeforeUtc)
            .Run();

        if (expired.Count == 0)
        {
            return 0;
        }

        var memberIds = expired
            .Select(x => x.MemberId)
            .Distinct()
            .ToArray();

        var (members, held) = await _unitOfWork.Run(
            x => x.MemberRepository.GetByIds(memberIds),
            x => x.MemberChapterInviteRepository.GetByMemberIds(memberIds));

        DiscardInvites(expired, members, held);

        await _unitOfWork.SaveChanges();

        return expired.Count;
    }

    public async Task<ServiceResult> RefuseInvite(IChapterServiceRequest request, string token)
    {
        var invite = await _unitOfWork.MemberChapterInviteRepository
            .GetByToken(token)
            .Run();

        if (invite == null || invite.ChapterId != request.Chapter.Id)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        var (member, held) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(invite.MemberId),
            x => x.MemberChapterInviteRepository.GetByMemberIds([invite.MemberId]));

        DiscardInvites([invite], [member], held);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// Deletes <paramref name="discarded"/>, then any of <paramref name="members"/> the discard leaves with
    /// nothing. <paramref name="held"/> is every invite those members hold, across all groups, so a member
    /// invited elsewhere survives losing this one.
    /// </summary>
    /// <remarks>
    /// Shared by the three ways an invite goes away - withdrawn, refused, purged - so what happens to the
    /// account behind it is one answer rather than three. Stages the writes and commits nothing.
    /// </remarks>
    private void DiscardInvites(
        IReadOnlyCollection<MemberChapterInvite> discarded,
        IReadOnlyCollection<Member> members,
        IReadOnlyCollection<MemberChapterInvite> held)
    {
        _unitOfWork.MemberChapterInviteRepository.DeleteMany(discarded);

        var discardedIds = discarded
            .Select(x => x.Id)
            .ToHashSet();

        // Grouped rather than scanned per member: a purge can carry as many members as invites.
        var heldByMemberId = held
            .GroupBy(x => x.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        /* An activated account is the member's own and outlives any invite. An unactivated one exists only
           because an import raised it, so it goes when the last invite that would have brought it to life
           does - leaving the group holding nothing about somebody who never replied. */
        var abandoned = members
            .Where(member => !member.Activated)
            .Where(member => !heldByMemberId.TryGetValue(member.Id, out var invites) ||
                             invites.All(invite => discardedIds.Contains(invite.Id)))
            .ToArray();

        _unitOfWork.MemberRepository.DeleteMany(abandoned);
    }
}
