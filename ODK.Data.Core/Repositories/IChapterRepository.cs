using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterRepository : IReadWriteRepository<Chapter>
{
    IDeferredQueryMultiple<Chapter> GetByAdminMemberId(Guid memberId);

    IDeferredQueryMultiple<Chapter> GetAll();

    IDeferredQuerySingle<Chapter> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<Chapter> GetByName(string name);

    IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId);

    IDeferredQueryMultiple<Chapter> GetByPlatform(PlatformType platform);

    IDeferredQueryMultiple<Chapter> GetByTopicGroupId(Guid topicGroupId);

    IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug);
}