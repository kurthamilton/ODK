using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(MemberChapterServiceRequest request, EmailType type);

    Task<ChapterEmail> GetChapterEmail(MemberChapterServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(MemberChapterServiceRequest request);

    Task<Email> GetEmail(Guid currentMemberId, EmailType type);
    
    Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId);

    Task<ServiceResult> SendTestEmail(MemberChapterServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(MemberServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(MemberChapterServiceRequest request, EmailType type, UpdateEmail model);

    Task<ServiceResult> UpdateEmail(Guid currentMemberId, EmailType type, UpdateEmail model);
}
