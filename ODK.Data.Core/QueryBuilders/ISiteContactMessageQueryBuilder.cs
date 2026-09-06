using ODK.Core.Messages;
using ODK.Core.Platforms;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteContactMessageQueryBuilder
    : IDatabaseEntityQueryBuilder<SiteContactMessage, ISiteContactMessageQueryBuilder>
{
    ISiteContactMessageQueryBuilder ForPlatform(PlatformType platform);

    ISiteContactMessageQueryBuilder ForStatus(MessageStatus status, double spamThreshold);
}