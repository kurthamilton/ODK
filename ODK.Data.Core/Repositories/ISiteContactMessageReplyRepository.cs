using ODK.Core.Messages;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteContactMessageReplyRepository : IReadWriteRepository<SiteContactMessageReply>
{
    IDeferredQueryMultiple<SiteContactMessageReply> GetAll();

    IDeferredQueryMultiple<SiteContactMessageReply> GetBySiteContactMessageId(Guid siteContactMessageId);
}
