using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.Core.Messages;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class SiteConversationDtoQueryBuilder
    : QueryBuilder<SiteConversationDto>, IQueryBuilder<SiteConversationDto>, ISiteConversationDtoQueryBuilder
{
    public SiteConversationDtoQueryBuilder(
        DbContext context, IQueryable<SiteConversation> siteConversationQuery)
        : base(context, BaseQuery(context, siteConversationQuery))
    {
    }

    public ISiteConversationDtoQueryBuilder ByLatestActivity()
    {
        Query = Query.OrderByDescending(x => x.LastMessage.Message.CreatedUtc);
        return this;
    }

    private static IQueryable<SiteConversationDto> BaseQuery(
        DbContext context, IQueryable<SiteConversation> siteConversationQuery)
    {
        var query =
            from conversation in siteConversationQuery
            from conversationMessage in context.Set<SiteConversationMessage>()
                .Where(x => x.SiteConversationId == conversation.Id)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(1)
            from conversationMessageMember in context.Set<Member>()
                .Where(x => x.Id == conversationMessage.MemberId)
            from member in context.Set<Member>()
                .Where(x => x.Id == conversation.MemberId)
            select new SiteConversationDto
            {
                Conversation = conversation,
                LastMessage = new SiteConversationMessageDto
                {
                    MemberFirstName = conversationMessageMember.FirstName,
                    MemberLastName = conversationMessageMember.LastName,
                    Message = conversationMessage
                },
                Member = member,
                MessageCount = context.Set<SiteConversationMessage>()
                    .Where(x => x.SiteConversationId == conversation.Id)
                    .Count()
            };

        return query;
    }
}
