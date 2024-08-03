using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(AdminServiceRequest request, EmailType type);

    Task<ChapterEmail> GetChapterEmail(AdminServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(AdminServiceRequest request);

    Task<ChapterEmailSettings?> GetChapterEmailSettings(AdminServiceRequest request);

    Task<Email> GetEmail(Guid currentMemberId, EmailType type);
    
    Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId);

    Task<ServiceResult> SendTestEmail(AdminServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(AdminServiceRequest request, EmailType type, UpdateEmail model);

    Task<ServiceResult> UpdateChapterEmailSettings(AdminServiceRequest request, UpdateChapterEmailSettings model);

    Task<ServiceResult> UpdateEmail(Guid currentMemberId, EmailType type, UpdateEmail model);
}
