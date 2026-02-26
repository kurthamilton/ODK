using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterLinksRepository : WriteRepositoryBase<ChapterLinks>, IChapterLinksRepository
{
    private readonly IChapterEntityRepository<ChapterLinks> _chapterEntityRepository;

    public ChapterLinksRepository(DbContext context)
        : base(context)
    {
        _chapterEntityRepository = new ChapterEntityRepositoryHelper<ChapterLinks>(this);
    }

    public IDeferredQuerySingleOrDefault<ChapterLinks> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    public void Upsert(ChapterLinks entity, Guid chapterId)
        => _chapterEntityRepository.Upsert(entity, chapterId);
}