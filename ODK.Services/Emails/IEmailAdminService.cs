using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(MemberChapterServiceRequest request, EmailType type);

    Task<ChapterEmail> GetChapterEmail(MemberChapterServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(MemberChapterServiceRequest request);

    Task<Email> GetEmail(MemberServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<Email>> GetEmails(MemberServiceRequest request);

    Task<ServiceResult> SendTestEmail(MemberChapterServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(MemberServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(MemberChapterServiceRequest request, EmailType type, UpdateEmail model);

    Task<ServiceResult> UpdateEmail(MemberServiceRequest request, EmailType type, UpdateEmail model);
}
