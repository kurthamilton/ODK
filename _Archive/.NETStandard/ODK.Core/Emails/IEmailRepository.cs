using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Emails
{
    public interface IEmailRepository
    {
        Task<Guid> AddChapterEmail(ChapterEmail chapterEmail);

        Task AddSentEmail(SentEmail sentEmail);

        Task DeleteChapterEmail(Guid chapterId, EmailType type);

        Task<ChapterEmail?> GetChapterEmail(Guid chapterId, EmailType type);

        Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid chapterId);

        Task<Email> GetEmail(EmailType type);

        Task<Email> GetEmail(EmailType type, Guid chapterId);

        Task<IReadOnlyCollection<Email>> GetEmails();

        Task<int> GetEmailsSentTodayCount(Guid chapterEmailProviderId);

        Task UpdateChapterEmail(ChapterEmail chapterEmail);

        Task UpdateEmail(Email email);
    }
}
