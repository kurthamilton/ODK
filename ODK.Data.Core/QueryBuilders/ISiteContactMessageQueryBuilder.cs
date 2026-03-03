using ODK.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteContactMessageQueryBuilder
    : IDatabaseEntityQueryBuilder<SiteContactMessage, ISiteContactMessageQueryBuilder>
{
    ISiteContactMessageQueryBuilder ForSpamScore(bool isSpam, double threshold);
}