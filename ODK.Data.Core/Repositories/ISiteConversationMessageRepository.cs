using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Messages;

namespace ODK.Data.Core.Repositories;

public interface ISiteConversationMessageRepository : IReadWriteRepository<SiteConversationMessage>
{
    IDeferredQueryMultiple<SiteConversationMessage> GetByConversationId(Guid siteConversationId);

    IDeferredQueryMultiple<SiteConversationMessageDto> GetDtosByConversationId(Guid siteConversationId);
}
