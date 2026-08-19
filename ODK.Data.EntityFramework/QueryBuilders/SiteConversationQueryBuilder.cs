using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.Messages;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class SiteConversationQueryBuilder
    : DatabaseEntityQueryBuilder<SiteConversation, ISiteConversationQueryBuilder>, ISiteConversationQueryBuilder
{
    public SiteConversationQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override ISiteConversationQueryBuilder Builder => this;

    public ISiteConversationQueryBuilder Archived(bool value)
    {
        Query = Query.Where(x => x.ArchivedUtc != null == value);
        return this;
    }

    public ISiteConversationQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public ISiteConversationDtoQueryBuilder ToDto() =>
        CreateQueryBuilder<ISiteConversationDtoQueryBuilder, SiteConversationDto>(
            context => new SiteConversationDtoQueryBuilder(context, Query));
}
