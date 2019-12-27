using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IEmailRepository
    {
        Task<Guid> AddChapterEmail(ChapterEmail chapterEmail);

        Task<ChapterEmail> GetChapterEmail(Guid chapterId, EmailType type);

        Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid chapterId);

        Task<Email> GetEmail(EmailType type);

        Task UpdateChapterEmail(ChapterEmail chapterEmail);
    }
}
