using ODK.Core;
using ODK.Core.Chapters;
using ODK.Data.Core;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberViewModelService : IMemberViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberPageViewModel> GetGroupMemberPage(MemberServiceRequest request, string slug, Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

        return await GetMemberPage(request, chapter, memberId);
    }

    public async Task<MembersPageViewModel> GetGroupMembersPage(MemberServiceRequest request, string slug)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

        return await GetMembersPage(request, chapter);
    }

    public async Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberServiceRequest request)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (currentMember, conversations) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(currentMemberId));

        var chapterIds = conversations
            .Select(x => x.Conversation.ChapterId)
            .Distinct()
            .ToArray();

        var chapters = chapterIds.Length > 0
            ? await _unitOfWork.ChapterRepository.GetByIds(chapterIds).Run()
            : [];

        return new MemberConversationsPageViewModel
        {
            Chapters = chapters,
            Conversations = conversations,
            CurrentMember = currentMember,
            Platform = platform
        };
    }

    public async Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberChapterServiceRequest request)
    {
        var (chapterId, currentMemberId, platform) = (request.ChapterId, request.CurrentMemberId, request.Platform);

        var (chapter, currentMember, conversations) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(currentMemberId, chapterId));

        return new MemberConversationsPageViewModel
        {
            Chapters = [chapter],
            Conversations = conversations,
            CurrentMember = currentMember,
            Platform = platform
        };
    }

    public async Task<MemberInterestsPageViewModel> GetMemberInterestsPage(Guid currentMemberId)
    {
        var (topicGroups, topics, memberTopics, newTopics) = await _unitOfWork.RunAsync(
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.MemberTopicRepository.GetByMemberId(currentMemberId),
            x => x.NewMemberTopicRepository.GetByMemberId(currentMemberId));

        return new MemberInterestsPageViewModel
        {
            MemberTopics = memberTopics,
            NewTopics = newTopics,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<MemberPageViewModel> GetMemberPage(MemberServiceRequest request, Chapter chapter, Guid memberId)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (
            currentMember,
            member,
            chapterProperties,
            memberProperties,
            hasQuestions,
            adminMembers,
            ownerSubscription,
            chapterPages) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        OdkAssertions.MemberOf(member, chapter.Id);

        return new MemberPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            ChapterProperties = chapterProperties,
            CurrentMember = currentMember,
            HasProfiles = chapterProperties.Any(),
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember.IsMemberOf(chapter.Id),
            Member = member,
            MemberProperties = memberProperties,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<MembersPageViewModel> GetMembersPage(MemberServiceRequest request, Chapter chapter)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        // get current member separately as they might be hidden from the list of members
        var (
            members,
            currentMember,
            hasProperties,
            hasQuestions,
            adminMembers,
            ownerSubscription,
            chapterPages) =
            await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        var isMember = currentMember.IsMemberOf(chapter.Id);

        return new MembersPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = isMember,
            Members = isMember ? members : [],
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }
}