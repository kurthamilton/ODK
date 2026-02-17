using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterImageRepository : IWriteRepository<ChapterImage>, IChapterEntityRepository<ChapterImage>
{
    IDeferredQuerySingleOrDefault<ChapterImage> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterImageVersionDto> GetVersionDtoByChapterId(Guid chapterId);
}