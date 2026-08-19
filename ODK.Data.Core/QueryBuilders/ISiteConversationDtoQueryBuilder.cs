using ODK.Data.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteConversationDtoQueryBuilder : IQueryBuilder<SiteConversationDto>
{
    /// <summary>
    /// Most recently active first, which is the order the site admin list is read in - a thread is
    /// interesting because somebody has just said something, not because of when it was opened.
    /// </summary>
    ISiteConversationDtoQueryBuilder ByLatestActivity();
}
