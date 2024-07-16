using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> AddChapterEmailProvider(Guid currentMemberId, Guid chapterId, UpdateChapterEmailProvider provider);

    Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

    Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, string chapterName, EmailType type);

    Task DeleteChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId);

    Task<ChapterEmail> GetChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

    Task<ChapterEmailProvider?> GetChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId);

    Task<IReadOnlyCollection<ChapterEmailProvider>> GetChapterEmailProviders(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId);

    Task<Email> GetEmail(Guid currentMemberId, Guid currentChapterId, EmailType type);
    
    Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId, Guid currentChapterId);

    Task<ServiceResult> UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateEmail chapterEmail);

    Task<ServiceResult> UpdateChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId, UpdateChapterEmailProvider provider);

    Task<ServiceResult> UpdateEmail(Guid currentMemberId, Guid currentChapterId, EmailType type, UpdateEmail email);
}
