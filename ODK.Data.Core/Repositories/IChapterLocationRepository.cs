using ODK.Core.Chapters;

namespace ODK.Data.Core.Repositories;

public interface IChapterLocationRepository : IWriteRepository<ChapterLocation>
{
    Task<IReadOnlyCollection<ChapterLocation>> GetAll();
    Task<ChapterLocation?> GetByChapterId(Guid chapterId);
}
