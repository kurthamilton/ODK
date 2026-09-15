using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Tasks;

namespace ODK.Services.Members;

public class MemberInviteService : IMemberInviteService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IServiceRequestFactory _serviceRequestFactory;
    private readonly MemberInviteServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberInviteService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IBackgroundTaskService backgroundTaskService,
        IServiceRequestFactory serviceRequestFactory,
        MemberInviteServiceSettings settings)
    {
        _backgroundTaskService = backgroundTaskService;
        _memberEmailService = memberEmailService;
        _serviceRequestFactory = serviceRequestFactory;
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

    public async Task<ServiceResult> RequestInviteResend(IChapterServiceRequest request, string emailAddress)
    {
        var chapter = request.Chapter;

        /* One result for every outcome, and the caller renders one wording. Anyone can put an address into
           the form this comes from, so saying whether there was an invite would say whether that person
           was in this group - which for a group that keeps its membership private is the fact it is
           keeping. The send is queued rather than awaited, so a hit and a miss take the same time. */
        var nothingToReport = ServiceResult.Successful();

        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(emailAddress)
            .Run();
        if (member == null)
        {
            return nothingToReport;
        }

        var invite = await _unitOfWork.MemberChapterInviteRepository
            .GetByMemberId(member.Id, chapter.Id)
            .Run();

        /* Only an invite the group has already emailed. One it is still holding is released by publishing
           the group, and letting a stranger's guess release it would take that decision away from the
           organisers. */
        if (invite?.IsResendable(_settings.ResendCooldownHours, DateTime.UtcNow) != true)
        {
            return nothingToReport;
        }

        invite.SentUtc = DateTime.UtcNow;
        _unitOfWork.MemberChapterInviteRepository.Update(invite);

        await _unitOfWork.SaveChanges();

        _backgroundTaskService.Enqueue(
            () => SendInviteEmailJob(JobRequest.Create(request), chapter.Id, member.Id),
            BackgroundTaskQueueType.Emails);

        return nothingToReport;
    }

    public async Task SendInviteEmail(IServiceRequest request, Guid chapterId, Guid memberId)
    {
        var (member, chapter, invite) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterRepository.GetById(request.Platform, chapterId),
            x => x.MemberChapterInviteRepository.GetByMemberId(memberId, chapterId));

        // Consumed once they join, and the link is worthless without it, so there is nothing to send.
        if (invite == null)
        {
            return;
        }

        var chapterRequest = ChapterServiceRequest.Create(chapter, request);
        await _memberEmailService.SendMemberImportInviteEmail(chapterRequest, member, invite.Token);
    }

    /* Public for Hangfire, which needs a method to bind to, and called by nothing else: it turns the job's
       ids back into a request and hands off to the work. This signature is a wire format - see JobRequest -
       so a change to it is a change every queued job of that kind has to survive. */
    public async Task SendInviteEmailJob(JobRequest request, Guid chapterId, Guid memberId)
        => await SendInviteEmail(await _serviceRequestFactory.Create(request), chapterId, memberId);

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
