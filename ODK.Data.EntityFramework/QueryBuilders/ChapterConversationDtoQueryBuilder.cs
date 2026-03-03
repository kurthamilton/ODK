using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterConversationDtoQueryBuilder
    : QueryBuilder<ChapterConversationDto>, IQueryBuilder<ChapterConversationDto>, IChapterConversationDtoQueryBuilder
{
    public ChapterConversationDtoQueryBuilder(
        DbContext context, IQueryable<ChapterConversation> chapterConversationQuery)
        : base(context, BaseQuery(context, chapterConversationQuery))
    {
    }

    public IChapterConversationDtoQueryBuilder ForStatus(MessageStatus status)
    {
        if (status == MessageStatus.Unread)
        {
            Query = Query.Where(x => !x.LastMessage.Message.ReadByChapter);
        }
        else if (status == MessageStatus.Read)
        {
            Query = Query.Where(x => x.LastMessage.Message.ReadByChapter);
        }

        return this;
    }

    private static IQueryable<ChapterConversationDto> BaseQuery(
        DbContext context, IQueryable<ChapterConversation> chapterConversationQuery)
    {
        var query =
            from conversation in chapterConversationQuery
            from conversationMessage in context.Set<ChapterConversationMessage>()
                .Where(x => x.ChapterConversationId == conversation.Id)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(1)
            from conversationMessageMember in context.Set<Member>()
                .Where(x => x.Id == conversationMessage.MemberId)
            from member in context.Set<Member>()
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
                Member = member,
                MessageCount = context.Set<ChapterConversationMessage>()
                    .Where(x => x.ChapterConversationId == conversation.Id)
                    .Count()
            };

        return query;
    }
}