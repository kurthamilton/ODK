using ODK.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteConversationQueryBuilder
    : IDatabaseEntityQueryBuilder<SiteConversation, ISiteConversationQueryBuilder>
{
    ISiteConversationQueryBuilder Archived(bool value);

    ISiteConversationQueryBuilder ForMember(Guid memberId);

    ISiteConversationDtoQueryBuilder ToDto();
}
