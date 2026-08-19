using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Messages;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface ISiteConversationRepository
    : IReadWriteRepository<SiteConversation, ISiteConversationQueryBuilder>
{
    /// <summary>
    /// Every conversation, most recently active first - the site admin inbox. Unfiltered by member on
    /// purpose: site admins are the other side of all of them.
    /// </summary>
    IDeferredQueryMultiple<SiteConversationDto> GetDtos(bool archived);

    IDeferredQueryMultiple<SiteConversationDto> GetDtosByMemberId(Guid memberId);
}
