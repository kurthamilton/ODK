using ODK.Core.Emails;
using ODK.Services.Emails.Models;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(MemberChapterAdminServiceRequest request, EmailType type);

    Task<ChapterEmail> GetChapterEmail(MemberChapterAdminServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(MemberChapterAdminServiceRequest request);

    Task<Email> GetEmail(MemberServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<Email>> GetEmails(MemberServiceRequest request);

    Task<ServiceResult> SendTestEmail(MemberChapterAdminServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(MemberServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(MemberChapterAdminServiceRequest request, EmailType type, EmailUpdateModel model);

    Task<ServiceResult> UpdateEmail(MemberServiceRequest request, EmailType type, EmailUpdateModel model);
}
