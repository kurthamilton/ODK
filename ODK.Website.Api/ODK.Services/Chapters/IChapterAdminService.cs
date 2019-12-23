using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question);

        Task<ChapterEmailSettings> GetChapterEmailSettings(Guid currentMemberId, Guid chapterId);

        Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId);

        Task<IReadOnlyCollection<string>> GetEmailProviders();

        Task UpdateChapterEmailSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailSettings emailSettings);

        Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links);

        Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId, UpdateChapterPaymentSettings paymentSettings);

        Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts);
    }
}
