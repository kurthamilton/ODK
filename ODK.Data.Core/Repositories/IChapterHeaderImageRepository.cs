using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterHeaderImageRepository
    : IWriteRepository<ChapterHeaderImage>, IChapterEntityRepository<ChapterHeaderImage>
{
    IDeferredQuerySingleOrDefault<ChapterHeaderImage> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterImageVersionDto> GetVersionDtoByChapterId(Guid chapterId);
}
