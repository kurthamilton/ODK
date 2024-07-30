using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

    Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, string chapterName, EmailType type);

    Task<ChapterEmail> GetChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId);

    Task<ChapterEmailSettings?> GetChapterEmailSettings(Guid currentMemberId, Guid chapterId);

    Task<Email> GetEmail(Guid currentMemberId, EmailType type);
    
    Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId);

    Task<ServiceResult> UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, UpdateEmail model);

    Task<ServiceResult> UpdateChapterEmailSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailSettings model);

    Task<ServiceResult> UpdateEmail(Guid currentMemberId, EmailType type, UpdateEmail model);
}
