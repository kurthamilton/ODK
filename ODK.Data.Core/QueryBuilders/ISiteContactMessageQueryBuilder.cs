using ODK.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteContactMessageQueryBuilder
    : IDatabaseEntityQueryBuilder<SiteContactMessage, ISiteContactMessageQueryBuilder>
{
    ISiteContactMessageQueryBuilder ForStatus(MessageStatus status, double spamThreshold);
}