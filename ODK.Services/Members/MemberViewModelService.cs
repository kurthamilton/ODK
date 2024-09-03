using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberViewModelService : IMemberViewModelService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public MemberViewModelService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberPageViewModel> GetGroupMemberPage(Guid currentMemberId, string slug, Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        return await GetMemberPage(currentMemberId, chapter, memberId);
    }

    public async Task<MembersPageViewModel> GetGroupMembersPage(Guid currentMemberId, string slug)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter);

        return await GetMembersPage(currentMemberId, chapter);
    }

    public async Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, string chapterName, Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).Run();
        OdkAssertions.Exists(chapter);

        return await GetMemberPage(currentMemberId, chapter, memberId);
    }

    public async Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, string chapterName)
    {        
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).Run();
        OdkAssertions.Exists(chapter);

        return await GetMembersPage(currentMemberId, chapter);
    }

    private async Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, Chapter chapter, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (
            currentMember, 
            members, 
            chapterProperties, 
            memberProperties,
            hasQuestions,
            adminMembers,
            ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetByChapterId(chapter.Id, [currentMemberId, memberId]),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        var member = members.FirstOrDefault(x => x.Id == memberId);

        OdkAssertions.MemberOf(currentMember, chapter.Id);
        OdkAssertions.Exists(member);

        return new MemberPageViewModel
        {
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember.IsMemberOf(chapter.Id),
            Member = member,
            MemberProperties = memberProperties,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    private async Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, Chapter chapter)
    {
        var platform = _platformProvider.GetPlatform();

        // get current member separately as they might be hidden from the list of members
        var (
            members, 
            currentMember,
            hasQuestions,
            adminMembers,
            ownerSubscription) = 
            await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        return new MembersPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember.IsMemberOf(chapter.Id),
            Members = members,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }
}
