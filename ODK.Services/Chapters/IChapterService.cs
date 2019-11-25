using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterService
    {
        Task<IReadOnlyCollection<Chapter>> GetAdminChapters(Guid memberId);

        Task<Chapter> GetChapter(Guid id);

        Task<ChapterLinks> GetChapterLinks(Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId);

        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters();        
    }
}
