using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
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

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId) => 
        ChapterConversationDtoSet(chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, bool replied) =>
        ChapterConversationDtoSet(chapterId)
            .Where(x => (x.Conversation.MemberId != x.LastMessage.MemberId) == replied)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId) =>
        ChapterConversationDtoSet(chapterId)
            .Where(x => x.Conversation.MemberId == memberId)
            .DeferredMultiple();

    private IQueryable<ChapterConversationDto> ChapterConversationDtoSet(Guid chapterId)
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
                .Where(x => x.MemberId == member.Id && x.ChapterId == conversation.ChapterId)
                .DefaultIfEmpty()
            where conversation.ChapterId == chapterId
                && member.Id == conversation.MemberId
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
