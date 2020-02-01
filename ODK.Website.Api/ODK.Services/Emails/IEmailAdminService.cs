using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails
{
    public interface IEmailAdminService
    {
        Task AddChapterEmailProvider(Guid currentMemberId, Guid chapterId, UpdateChapterEmailProvider provider);

        Task DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

        Task DeleteChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId);

        Task<ChapterEmail> GetChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

        Task<ChapterEmailProvider> GetChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId);

        Task<IReadOnlyCollection<ChapterEmailProvider>> GetChapterEmailProviders(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId);

        Task<Email> GetEmail(Guid currentMemberId, Guid currentChapterId, EmailType type);

        Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId, Guid currentChapterId);

        Task UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateEmail chapterEmail);

        Task UpdateChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId, UpdateChapterEmailProvider provider);

        Task UpdateEmail(Guid currentMemberId, Guid currentChapterId, EmailType type, UpdateEmail email);
    }
}
