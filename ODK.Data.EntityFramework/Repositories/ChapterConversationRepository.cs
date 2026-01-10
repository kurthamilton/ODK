using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterConversationRepository : ReadWriteRepositoryBase<ChapterConversation>, IChapterConversationRepository
{
    public ChapterConversationRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterConversation> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversation> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId)
        => ChapterConversationDtoSet()
            .Where(x => x.Conversation.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, bool readByChapter)
        => ChapterConversationDtoSet()
            .Where(x => x.Conversation.ChapterId == chapterId &&
                x.LastMessage.ReadByChapter == readByChapter)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId)
        => ChapterConversationDtoSet()
            .Where(x => x.Conversation.MemberId == memberId)
            .DeferredMultiple();


    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId)
        => ChapterConversationDtoSet()
            .Where(x => x.Conversation.ChapterId == chapterId && x.Conversation.MemberId == memberId)
            .DeferredMultiple();

    private IQueryable<ChapterConversationDto> ChapterConversationDtoSet()
    {
        var query =
            from conversation in Set()
            from conversationMessage in Set<ChapterConversationMessage>()
                .Include(x => x.Member)
                .ThenInclude(x => x!.Chapters)
                .Where(x => x.ChapterConversationId == conversation.Id)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(1)
            from member in Set<Member>().Include(x => x.Chapters)
            from memberSubscription in Set<MemberSubscription>()
                .Where(x => x.MemberChapter.MemberId == member.Id && x.MemberChapter.ChapterId == conversation.ChapterId)
                .DefaultIfEmpty()
            where member.Id == conversation.MemberId
            select new ChapterConversationDto
            {
                Conversation = conversation,
                LastMessage = conversationMessage,
                Member = member,
                MemberSubscription = memberSubscription
            };

        return query;
    }
}
