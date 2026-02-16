using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterImageRepository : WriteRepositoryBase<ChapterImage>, IChapterImageRepository
{
    private readonly ChapterEntityRepositoryHelper<ChapterImage> _chapterEntityRepository;

    public ChapterImageRepository(OdkContext context)
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterImage>(this);
    }

    public IDeferredQuery<bool> ExistsForChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredAny();

    public IDeferredQuerySingleOrDefault<ChapterImage> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<ChapterImageMetadataDto> GetMetadatasByChapterIds(IEnumerable<Guid> chapterIds)
        => Set()
            .Where(x => chapterIds.Contains(x.ChapterId))
            .Metadata()
            .DeferredMultiple();

    public void Upsert(ChapterImage entity, Guid chapterId)
    {
        _chapterEntityRepository.Upsert(entity, chapterId);
    }
}