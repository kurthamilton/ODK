using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IEmailRepository
    {
        Task<Guid> AddChapterEmail(ChapterEmail chapterEmail);

        Task DeleteChapterEmail(Guid chapterId, EmailType type);

        Task<ChapterEmail> GetChapterEmail(Guid chapterId, EmailType type);

        Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid chapterId);

        Task<Email> GetEmail(Guid chapterId, EmailType type);

        Task UpdateChapterEmail(ChapterEmail chapterEmail);
    }
}
