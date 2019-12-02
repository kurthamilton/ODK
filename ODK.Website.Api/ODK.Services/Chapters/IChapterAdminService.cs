using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId);

        Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);
    }
}
