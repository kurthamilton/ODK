using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterRepository : IReadWriteRepository<Chapter>
{
    IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform);

    IDeferredQueryMultiple<Chapter> GetByAdminMemberId(PlatformType platformType, Guid memberId);

    IDeferredQuerySingle<Chapter> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<Chapter> GetByName(string name);

    IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId);

    IDeferredQueryMultiple<Chapter> GetByTopicGroupId(PlatformType platform, Guid topicGroupId);

    IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug);
}