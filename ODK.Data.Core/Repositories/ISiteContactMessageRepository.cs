using ODK.Core.Messages;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface ISiteContactMessageRepository : IReadWriteRepository<SiteContactMessage, ISiteContactMessageQueryBuilder>
{
}