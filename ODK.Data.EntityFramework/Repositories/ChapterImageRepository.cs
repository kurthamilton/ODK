using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterImageRepository : WriteRepositoryBase<ChapterImage>, IChapterImageRepository
{
    private readonly ChapterEntityRepositoryHelper<ChapterImage> _chapterEntityRepository;

    public ChapterImageRepository(DbContext context)
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterImage>(this);
    }

    public IDeferredQuerySingleOrDefault<ChapterImage> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<ChapterImageVersionDto> GetVersionDtoByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .Select(x => new ChapterImageVersionDto
            {
                Version = x.VersionInt
            })
            .DeferredSingleOrDefault();

    public void Upsert(ChapterImage entity, Guid chapterId)
    {
        _chapterEntityRepository.Upsert(entity, chapterId);
    }
}