using ODK.Core.Emails;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class EmailRepository : RepositoryBase, IEmailRepository
{
    public EmailRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task<Guid> AddChapterEmailAsync(ChapterEmail chapterEmail)
    {
        return await Context
            .Insert(chapterEmail)
            .GetIdentityAsync();
    }

    public async Task AddSentEmailAsync(SentEmail sentEmail)
    {
        await Context
            .Insert(sentEmail)
            .ExecuteAsync();
    }

    public async Task DeleteChapterEmailAsync(Guid chapterId, EmailType type)
    {
        await Context
            .Delete<ChapterEmail>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .Where(x => x.Type).EqualTo(type)
            .ExecuteAsync();
    }

    public async Task<ChapterEmail?> GetChapterEmailAsync(Guid chapterId, EmailType type)
    {
        return await Context
            .Select<ChapterEmail>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .Where(x => x.Type).EqualTo(type)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmailsAsync(Guid chapterId)
    {
        return await Context
            .Select<ChapterEmail>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .ToArrayAsync();
    }

    public async Task<Email> GetEmailAsync(EmailType type)
    {
        return await Context
            .Select<Email>()
            .Where(x => x.Type).EqualTo(type)
            .FirstOrDefaultAsync();
    }

    public async Task<Email> GetEmailAsync(EmailType type, Guid chapterId)
    {
        var chapterEmail = await GetChapterEmailAsync(chapterId, type);
        if (chapterEmail != null)
        {
            return new Email(chapterEmail.Type, chapterEmail.Subject, chapterEmail.HtmlContent);
        }

        return await GetEmailAsync(type);
    }

    public async Task<IReadOnlyCollection<Email>> GetEmailsAsync()
    {
        return await Context
            .Select<Email>()
            .ToArrayAsync();
    }

    public async Task<int> GetEmailsSentTodayCountAsync(Guid chapterEmailProviderId)
    {
        return await Context
            .Select<SentEmail>()
            .Where(x => x.ChapterEmailProviderId).EqualTo(chapterEmailProviderId)
            .Where(x => x.SentDate).GreaterThanOrEqualTo(DateTime.Today)
            .Where(x => x.SentDate).LessThan(DateTime.Today.AddDays(1))
            .CountAsync();
    }

    public async Task UpdateChapterEmailAsync(ChapterEmail chapterEmail)
    {
        await Context
            .Update<ChapterEmail>()
            .Set(x => x.HtmlContent, chapterEmail.HtmlContent)
            .Set(x => x.Subject, chapterEmail.Subject)
            .Where(x => x.Id).EqualTo(chapterEmail.Id)
            .ExecuteAsync();
    }

    public async Task UpdateEmailAsync(Email email)
    {
        await Context
            .Update<Email>()
            .Set(x => x.HtmlContent, email.HtmlContent)
            .Set(x => x.Subject, email.Subject)
            .Where(x => x.Type).EqualTo(email.Type)
            .ExecuteAsync();
    }
}
