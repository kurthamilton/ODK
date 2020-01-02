using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Emails;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class EmailRepository : RepositoryBase, IEmailRepository
    {
        public EmailRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<Guid> AddChapterEmail(ChapterEmail chapterEmail)
        {
            return await Context
                .Insert(chapterEmail)
                .GetIdentityAsync();
        }

        public async Task DeleteChapterEmail(Guid chapterId, EmailType type)
        {
            await Context
                .Delete<ChapterEmail>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Type).EqualTo(type)
                .ExecuteAsync();
        }

        public async Task<ChapterEmail> GetChapterEmail(Guid chapterId, EmailType type)
        {
            return await Context
                .Select<ChapterEmail>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .Where(x => x.Type).EqualTo(type)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid chapterId)
        {
            return await Context
                .Select<ChapterEmail>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<Email> GetEmail(EmailType type)
        {
            return await Context
                .Select<Email>()
                .Where(x => x.Type).EqualTo(type)
                .FirstOrDefaultAsync();
        }

        public async Task<Email> GetEmail(EmailType type, Guid chapterId)
        {
            ChapterEmail chapterEmail = await GetChapterEmail(chapterId, type);
            if (chapterEmail != null)
            {
                return new Email(chapterEmail.Type, chapterEmail.Subject, chapterEmail.HtmlContent);
            }

            return await GetEmail(type);
        }

        public async Task<IReadOnlyCollection<Email>> GetEmails()
        {
            return await Context
                .Select<Email>()
                .ToArrayAsync();
        }

        public async Task UpdateChapterEmail(ChapterEmail chapterEmail)
        {
            await Context
                .Update<ChapterEmail>()
                .Set(x => x.HtmlContent, chapterEmail.HtmlContent)
                .Set(x => x.Subject, chapterEmail.Subject)
                .Where(x => x.Id).EqualTo(chapterEmail.Id)
                .ExecuteAsync();
        }

        public async Task UpdateEmail(Email email)
        {
            await Context
                .Update<Email>()
                .Set(x => x.HtmlContent, email.HtmlContent)
                .Set(x => x.Subject, email.Subject)
                .Where(x => x.Type).EqualTo(email.Type)
                .ExecuteAsync();
        }
    }
}
