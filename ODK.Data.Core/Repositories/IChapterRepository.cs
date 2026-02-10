using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterRepository : IWriteRepository<Chapter>
{
    IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform, bool includeUnpublished);

    IDeferredQueryMultiple<Chapter> GetApproved(PlatformType platform);

    IDeferredQueryMultiple<Chapter> GetByAdminMemberId(PlatformType platform, Guid memberId);

    IDeferredQuerySingle<Chapter> GetByEventId(PlatformType platform, Guid eventId);

    IDeferredQuerySingle<Chapter> GetById(PlatformType platform, Guid id);

    IDeferredQuerySingleOrDefault<Chapter> GetByIdOrDefault(PlatformType platform, Guid id);

    IDeferredQueryMultiple<Chapter> GetByIds(PlatformType platform, IEnumerable<Guid> ids);

    IDeferredQueryMultiple<Chapter> GetByMemberId(PlatformType platform, Guid memberId);

    IDeferredQuerySingleOrDefault<Chapter> GetByName(PlatformType platform, string name);

    IDeferredQueryMultiple<Chapter> GetByOwnerId(PlatformType platform, Guid ownerId);

    IDeferredQueryMultiple<Chapter> GetByTopicGroupId(PlatformType platform, Guid topicGroupId);

    IDeferredQuerySingleOrDefault<Chapter> GetBySlug(PlatformType platform, string slug);

    IDeferredQuery<bool> NameExists(string name);

    IDeferredQuery<bool> SlugExists(string slug);
}