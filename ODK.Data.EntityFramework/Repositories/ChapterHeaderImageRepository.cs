using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterHeaderImageRepository
    : WriteRepositoryBase<ChapterHeaderImage>, IChapterHeaderImageRepository
{
    private readonly ChapterEntityRepositoryHelper<ChapterHeaderImage> _chapterEntityRepository;

    public ChapterHeaderImageRepository(DbContext context)
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterHeaderImage>(this);
    }

    public IDeferredQuerySingleOrDefault<ChapterHeaderImage> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<ChapterImageVersionDto> GetVersionDtoByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .Select(x => new ChapterImageVersionDto
            {
                ChapterId = x.ChapterId,
                Version = x.VersionInt
            })
            .DeferredSingleOrDefault();

    public void Upsert(ChapterHeaderImage entity, Guid chapterId)
    {
        _chapterEntityRepository.Upsert(entity, chapterId);
    }
}
