using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : CachingReadWriteRepositoryBase<Chapter>, IChapterRepository, IDisposable
{
    private static readonly EntityCache<Guid, Chapter> _cache = new DatabaseEntityCache<Chapter>();

    public ChapterRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Chapter> GetAll() => Set()
        .DeferredMultiple(           
            _cache.GetAll,
            _cache.SetAll);

    public IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId)
    {
        var query =
            from chapter in Set()
            from memberChapter in Set<MemberChapter>()
            where memberChapter.MemberId == memberId
                && memberChapter.ChapterId == chapter.Id
            select chapter;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault(
            () => _cache.Find(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)),
            _cache.Set,
            _cache.SetAll);

    public IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId) => Set()
        .Where(x => x.OwnerId == ownerId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug) => Set()
        .Where(x => x.Slug == slug)
        .DeferredSingleOrDefault(
            () => _cache.Find(x => string.Equals(x.Slug, slug, StringComparison.InvariantCultureIgnoreCase)),
            _cache.Set,
            _cache.SetAll);
}
