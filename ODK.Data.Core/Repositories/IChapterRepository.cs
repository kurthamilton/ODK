using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

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

    IDeferredQuerySingleOrDefault<Chapter> GetBySlug(PlatformType platform, string slug);

    IDeferredQueryMultiple<ChapterDto> GetDtosByMemberId(PlatformType platform, Guid memberId);

    /// <summary>
    /// The published chapters this platform owns. For work that acts on a group rather than displaying it -
    /// a scheduled sweep, a reminder - so that exactly one deployment does it: what a platform *shows* takes
    /// in the other's groups on Group Squirrel, which would have both deployments act on the same Drunken
    /// Knitwits group.
    /// </summary>
    IDeferredQueryMultiple<Chapter> GetOwnedByPlatform(PlatformType platform);

    IDeferredQuery<bool> NameExists(string name);

    /// <summary>
    /// Every chapter, whatever platform owns it. For work that is about no one site - a uniqueness check, or
    /// a lookup by an id that already names the chapter. Prefer an overload that names a platform wherever
    /// the answer should depend on one.
    /// </summary>
    IChapterQueryBuilder Query();

    /// <summary>The chapters <paramref name="platform"/> shows, published only.</summary>
    IChapterQueryBuilder Query(PlatformType platform);

    /// <summary>The chapters <paramref name="platform"/> shows.</summary>
    IChapterQueryBuilder Query(PlatformType platform, bool includeUnpublished);

    IDeferredQueryMultiple<ChapterSearchResultDto> Search(PlatformType platform, ChapterSearchCriteria criteria);

    IDeferredQuery<bool> SlugExists(string slug);
}