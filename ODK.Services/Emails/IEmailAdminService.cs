using ODK.Core.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.ViewModels;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ServiceResult> DeleteChapterEmail(IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ChapterEmailAdminPageViewModel> GetChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ChapterEmailsAdminPageViewModel> GetChapterEmails(IMemberChapterAdminServiceRequest request);

    Task<Email> GetEmail(IMemberServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<Email>> GetEmails(IMemberServiceRequest request);

    Task<ServiceResult> SendTestEmail(IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(IMemberServiceRequest request, EmailType type);

    Task<ServiceResult> UpdateChapterEmail(IMemberChapterAdminServiceRequest request, EmailType type, EmailUpdateModel model);

    /// <summary>
    /// Sets the group's own audience titles. A blank value is stored as unset, which is how the group goes
    /// back to inheriting the site's.
    /// </summary>
    Task<ServiceResult> UpdateChapterEmailSettings(
        IMemberChapterAdminServiceRequest request, ChapterEmailSettingsUpdateModel model);

    Task<ServiceResult> UpdateEmail(IMemberServiceRequest request, EmailType type, EmailUpdateModel model);

    /// <summary>
    /// The HTML check <see cref="UpdateChapterEmail"/> applies, without writing anything, so the editor
    /// can run it while the admin types. Only the markup rules: placeholders are checked in the browser
    /// already, and reporting them here too would flag the same field twice.
    /// </summary>
    Task<ServiceResult> ValidateChapterEmailHtml(
        IMemberChapterAdminServiceRequest request, EmailType type, string? htmlContent);

    /// <inheritdoc cref="ValidateChapterEmailHtml" />
    ServiceResult ValidateEmailHtml(IMemberServiceRequest request, EmailType type, string? htmlContent);
}