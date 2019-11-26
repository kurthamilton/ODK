using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterService
    {
        Task<Chapter> GetChapter(Guid id);

        Task<ChapterLinks> GetChapterLinks(Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId);

        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);

        Task<VersionedServiceResult<IReadOnlyCollection<Chapter>>> GetChapters(int? currentVersion);
    }
}
