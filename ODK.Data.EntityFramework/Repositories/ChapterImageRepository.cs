using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

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

    public IDeferredQueryMultiple<ChapterImageMetadata> GetDtosByChapterIds(IEnumerable<Guid> chapterIds)
    {
        var query =
            from chapter in Set<Chapter>()
            from image in Set()
            where image.ChapterId == chapter.Id
                && chapterIds.Contains(chapter.Id)
            select new ChapterImageMetadata
            {
                ChapterId = chapter.Id,
                MimeType = image.MimeType
            };
        return query.DeferredMultiple();
    }

    public void Upsert(ChapterImage entity, Guid chapterId)
    {
        _chapterEntityRepository.Upsert(entity, chapterId);
    }
}
