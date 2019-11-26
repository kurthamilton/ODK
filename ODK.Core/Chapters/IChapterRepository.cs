using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Chapters
{
    public interface IChapterRepository
    {
        Task<IReadOnlyCollection<Chapter>> GetAdminChapters(Guid memberId);

        Task<Chapter> GetChapter(Guid id);

        Task<ChapterAdminMember> GetChapterAdminMember(Guid chapterId, Guid memberId);

        Task<ChapterEmailSettings> GetChapterEmailSettings(Guid chapterId);

        Task<ChapterLinks> GetChapterLinks(Guid chapterId);

        Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId);

        Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters();
    }
}
