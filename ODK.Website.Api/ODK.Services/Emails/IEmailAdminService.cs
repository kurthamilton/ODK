using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails
{
    public interface IEmailAdminService
    {
        Task DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

        Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId, Guid currentChapterId);

        Task UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateEmail chapterEmail);

        Task UpdateChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailProviderSettings emailProviderSettings);

        Task UpdateEmail(Guid currentMemberId, Guid currentChapterId, EmailType type, UpdateEmail email);
    }
}
