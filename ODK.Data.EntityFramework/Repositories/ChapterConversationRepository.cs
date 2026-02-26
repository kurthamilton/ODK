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
    public ChapterConversationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, bool readByChapter)
        => ChapterConversationDtoSet()
            .Where(x => x.Conversation.ChapterId == chapterId &&
                x.LastMessage.Message.ReadByChapter == readByChapter)
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
                .Where(x => x.ChapterConversationId == conversation.Id)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(1)
            from conversationMessageMember in Set<Member>()
                .Where(x => x.Id == conversationMessage.MemberId)
            from member in Set<Member>()
                .Include(x => x.Chapters)
                .Where(x => x.Id == conversation.MemberId)
            select new ChapterConversationDto
            {
                Conversation = conversation,
                LastMessage = new ChapterConversationMessageDto
                {
                    MemberFirstName = conversationMessageMember.FirstName,
                    MemberLastName = conversationMessageMember.LastName,
                    Message = conversationMessage
                },
                Member = member
            };

        return query;
    }
}