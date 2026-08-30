using ODK.Core.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.ViewModels;

namespace ODK.Services.Emails;

public interface IEmailAdminService
{
    Task<ChapterEmailAdminPageViewModel> GetChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ChapterEmailsAdminPageViewModel> GetChapterEmails(IMemberChapterAdminServiceRequest request);

    Task<EmailAdminPageViewModel> GetEmail(IMemberServiceRequest request, EmailType type);

    Task<IReadOnlyCollection<Email>> GetEmails(IMemberServiceRequest request);

    /// <summary>
    /// Renders one of a group's emails as the form currently holds it, resolving each field the way
    /// <see cref="UpdateChapterEmail"/> would, so an admin sees what saving would send.
    /// </summary>
    Task<RenderedEmail> PreviewChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type, ChapterEmailUpdateModel model);

    /// <inheritdoc cref="PreviewChapterEmail" />
    Task<RenderedEmail> PreviewEmail(
        IMemberServiceRequest request, EmailType type, string subject, string body);

    Task<ServiceResult> SendTestEmail(IMemberChapterAdminServiceRequest request, EmailType type);

    Task<ServiceResult> SendTestMemberEmail(IMemberServiceRequest request, EmailType type);

    /// <summary>
    /// Sets the group's override of one email. Subject and body are independent: a blank field is stored as
    /// unset, which is how the group goes back to sending the site's. Blanking both removes the override.
    /// </summary>
    Task<ServiceResult> UpdateChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type, ChapterEmailUpdateModel model);

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
        IMemberChapterAdminServiceRequest request, EmailType type, string? bodyHtml);

    /// <inheritdoc cref="ValidateChapterEmailHtml" />
    ServiceResult ValidateEmailHtml(IMemberServiceRequest request, EmailType type, string? bodyHtml);
}