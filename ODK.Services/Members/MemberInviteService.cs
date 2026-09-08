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

        var abandoned = members
            .Where(member => !member.Activated)
            .Where(member => !heldByMemberId.TryGetValue(member.Id, out var invites) ||
                             invites.All(invite => discardedIds.Contains(invite.Id)))
            .ToArray();

        _unitOfWork.MemberRepository.DeleteMany(abandoned);
    }
}
