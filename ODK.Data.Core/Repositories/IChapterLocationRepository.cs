using ODK.Core.Chapters;

namespace ODK.Data.Core.Repositories;

public interface IChapterLocationRepository : IWriteRepository<ChapterLocation>
{
    Task<ChapterLocation?> GetByChapterId(Guid chapterId);
    Task<IReadOnlyCollection<ChapterLocation>> GetByChapterIds(IEnumerable<Guid> chapterIds);
}
