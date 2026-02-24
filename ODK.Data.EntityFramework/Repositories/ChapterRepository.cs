using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : WriteRepositoryBase<Chapter>, IChapterRepository
{
    public ChapterRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Chapter> GetAll(PlatformType platform, bool includeUnpublished)
        => Query(platform, includeUnpublished)
            .GetAll();

    public IDeferredQueryMultiple<Chapter> GetApproved(PlatformType platform)
        => Query(platform, includeUnpublished: false)
            .Approved()
            .GetAll();

    public IDeferredQueryMultiple<Chapter> GetByAdminMemberId(PlatformType platform, Guid memberId)
        => Query(platform, includeUnpublished: true)
            .ForAdminMember(memberId)
            .GetAll();

    public IDeferredQuerySingle<Chapter> GetByEventId(PlatformType platform, Guid eventId)
        => Query(platform, includeUnpublished: false)
            .ForEvent(eventId)
            .GetSingle();

    public IDeferredQuerySingle<Chapter> GetById(PlatformType platform, Guid id)
        => Query(platform, includeUnpublished: true)
            .ById(id)
            .GetSingle();

    public IDeferredQuerySingleOrDefault<Chapter> GetByIdOrDefault(PlatformType platform, Guid id)
        => Query(platform, includeUnpublished: true)
            .ById(id)
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByIds(PlatformType platform, IEnumerable<Guid> ids)
        => Query(platform, includeUnpublished: true)
            .ByIds(ids)
            .GetAll();

    public IDeferredQueryMultiple<Chapter> GetByMemberId(PlatformType platform, Guid memberId)
        => Query(platform, includeUnpublished: true)
            .ForMember(memberId)
            .GetAll();

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(PlatformType platform, string name)
        => Query(platform, includeUnpublished: false)
            .ForName(name)
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<Chapter> GetByOwnerId(PlatformType platform, Guid ownerId)
        => Query(platform, includeUnpublished: true)
            .ForOwner(ownerId)
            .GetAll();

    public IDeferredQuerySingleOrDefault<Chapter> GetBySlug(PlatformType platform, string slug)
        => Query(platform, includeUnpublished: false)
            .ForSlug(slug)
            .GetSingleOrDefault();

    public IDeferredQueryMultiple<ChapterDto> GetDtosByMemberId(PlatformType platform, Guid memberId)
        => Query(platform, includeUnpublished: true)
            .ForMember(memberId)
            .ToChapterDto()
            .GetAll();

    public IDeferredQuery<bool> NameExists(string name)
        => Query(PlatformType.Default, includeUnpublished: true)
            .ForName(name)
            .Any();

    public IChapterQueryBuilder Query(PlatformType platform)
        => Query(platform, includeUnpublished: false);

    public IChapterQueryBuilder Query(PlatformType platform, bool includeUnpublished)
        => CreateQueryBuilder<IChapterQueryBuilder, Chapter>(context
            => new ChapterQueryBuilder(context, platform, includeUnpublished));

    public IDeferredQueryMultiple<ChapterSearchResultDto> Search(PlatformType platform, ChapterSearchCriteria criteria)
        => Query(platform, includeUnpublished: false)
            .Search(criteria)
            .GetAll();

    public IDeferredQuery<bool> SlugExists(string slug)
        => Query(PlatformType.Default, includeUnpublished: true)
            .ForSlug(slug)
            .Any();
}