using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.Repositories;

public interface IEventHostRepository : IReadWriteRepository<EventHost>
{
    IDeferredQueryMultiple<EventHost> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<MemberWithAvatarDto> GetByEventShortcode(string shortcode);
}