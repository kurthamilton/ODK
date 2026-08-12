using ODK.Core.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.ViewModels;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ChapterEmailAdminPageViewModel> GetChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(IMemberChapterAdminServiceRequest request);

    Task<Email> GetEmail(IMemberServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<Email>> GetEmails(IMemberServiceRequest request);

    Task<ServiceResult> SendTestEmail(IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(IMemberServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(IMemberChapterAdminServiceRequest request, EmailType type, EmailUpdateModel model);

    Task<ServiceResult> UpdateEmail(IMemberServiceRequest request, EmailType type, EmailUpdateModel model);
}