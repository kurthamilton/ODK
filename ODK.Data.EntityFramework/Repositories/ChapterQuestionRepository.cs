using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterQuestionRepository : ReadWriteRepositoryBase<ChapterQuestion>, IChapterQuestionRepository
{
    public ChapterQuestionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuery<bool> ChapterHasQuestions(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredAny();

    public IDeferredQueryMultiple<ChapterQuestion> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderBy(x => x.DisplayOrder)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterQuestionCountDto> GetCountsByChapterIds(IEnumerable<Guid> chapterIds)
        => Set()
            .Where(x => chapterIds.Contains(x.ChapterId))
            .GroupBy(x => x.ChapterId)
            .Select(x => new ChapterQuestionCountDto
            {
                ChapterId = x.Key,
                Count = x.Count()
            })
            .DeferredMultiple();
}