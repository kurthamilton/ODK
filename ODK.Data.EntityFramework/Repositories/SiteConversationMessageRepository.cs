using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Messages;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteConversationMessageRepository
    : ReadWriteRepositoryBase<SiteConversationMessage>, ISiteConversationMessageRepository
{
    public SiteConversationMessageRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteConversationMessage> GetByConversationId(Guid siteConversationId)
        => Set()
            .Where(x => x.SiteConversationId == siteConversationId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<SiteConversationMessageDto> GetDtosByConversationId(Guid siteConversationId)
    {
        var query =
            from message in Set()
            from member in Set<Member>()
                .Where(x => x.Id == message.MemberId)
            where message.SiteConversationId == siteConversationId
            select new SiteConversationMessageDto
            {
                MemberFirstName = member.FirstName,
                MemberLastName = member.LastName,
                Message = message
            };

        return query.DeferredMultiple();
    }
}
