namespace ODK.Core.Emails;

public interface IEmailRepository
{
    Task<Guid> AddChapterEmailAsync(ChapterEmail chapterEmail);

    Task AddSentEmailAsync(SentEmail sentEmail);

    Task DeleteChapterEmailAsync(Guid chapterId, EmailType type);

    Task<ChapterEmail?> GetChapterEmailAsync(Guid chapterId, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmailsAsync(Guid chapterId);

    Task<Email?> GetEmailAsync(EmailType type);

    Task<Email?> GetEmailAsync(EmailType type, Guid chapterId);

    Task<IReadOnlyCollection<Email>> GetEmailsAsync();

    Task<int> GetEmailsSentTodayCountAsync(Guid chapterEmailProviderId);

    Task UpdateChapterEmailAsync(ChapterEmail chapterEmail);

    Task UpdateEmailAsync(Email email);
}
