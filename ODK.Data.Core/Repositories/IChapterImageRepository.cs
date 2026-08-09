using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterImageRepository : IWriteRepository<ChapterImage>, IChapterEntityRepository<ChapterImage>
{
    IDeferredQuerySingleOrDefault<ChapterImage> GetByChapterId(Guid chapterId);

    /// <summary>
    /// Image versions for whichever of <paramref name="chapterIds"/> have one - a chapter without an image
    /// simply has no row. Batched deliberately: the caller asks about every chapter a member owns, and a
    /// per-chapter query would be an N+1.
    /// </summary>
    IDeferredQueryMultiple<ChapterImageVersionDto> GetVersionDtosByChapterIds(
        IReadOnlyCollection<Guid> chapterIds);

    IDeferredQuerySingleOrDefault<ChapterImageVersionDto> GetVersionDtoByChapterId(Guid chapterId);
}