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

    public async Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var conversations = await _unitOfWork.ChapterConversationRepository.GetDtosByMemberId(currentMember.Id).Run();

        var chapterIds = conversations
            .Select(x => x.Conversation.ChapterId)
            .Distinct()
            .ToArray();

        var chapters = chapterIds.Length > 0
            ? await _unitOfWork.ChapterRepository.GetByIds(platform, chapterIds).Run()
            : [];

        return new MemberConversationsPageViewModel
        {
            Chapters = chapters,
            Conversations = conversations,
            CurrentMember = currentMember,
            Platform = platform
        };
    }

    public async Task<MemberConversationsPageViewModel> GetMemberConversationsPage(IMemberChapterServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var conversations = await _unitOfWork.ChapterConversationRepository
            .GetDtosByMemberId(currentMember.Id, chapter.Id)
            .Run();

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

    public async Task<MemberPageViewModel> GetMemberPage(IMemberChapterServiceRequest request, Guid memberId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            member,
            chapterProperties,
            memberProperties,
            hasQuestions,
            isAdmin,
            chapterPages) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.IsAdmin(platform, chapter.Id, currentMember.Id),
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
            IsAdmin = isAdmin,
            IsMember = currentMember.IsMemberOf(chapter.Id),
            Member = member,
            MemberProperties = memberProperties,
            Platform = platform
        };
    }

    public async Task<MembersPageViewModel> GetMembersPage(IMemberChapterServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        // get current member separately as they might be hidden from the list of members
        var (
            members,
            hasProperties,
            hasQuestions,
            isAdmin,
            chapterPages) =
            await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.IsAdmin(platform, chapter.Id, currentMember.Id),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

        var isMember = currentMember.IsMemberOf(chapter.Id);

        return new MembersPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            CurrentMember = currentMember,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            IsAdmin = isAdmin,
            IsMember = isMember,
            Members = isMember ? members : [],
            Platform = platform
        };
    }
}