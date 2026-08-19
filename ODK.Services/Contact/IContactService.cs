using ODK.Services.Contact.ViewModels;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Contact;

public interface IContactService
{
    Task<ServiceResult> ArchiveChapterConversation(IMemberServiceRequest request, Guid conversationId);

    Task<ServiceResult> ArchiveSiteConversation(IMemberServiceRequest request, Guid conversationId);

    Task<ContactPageViewModel> GetContactPageViewModel(IServiceRequest request);

    Task<SiteConversationPageViewModel> GetSiteConversationPage(
        IMemberServiceRequest request, Guid conversationId);

    Task<SiteConversationsPageViewModel> GetSiteConversationsPage(IMemberServiceRequest request, bool archived);

    Task<ServiceResult> ReplyToChapterConversation(IMemberServiceRequest request, Guid conversationId, string message);

    Task<ServiceResult> ReplyToSiteConversation(
        IMemberServiceRequest request, Guid conversationId, string message);

    Task SendChapterContactMessage(
        IChapterServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken);

    Task SendSiteContactMessage(
        IServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken);

    Task<ServiceResult> StartChapterConversation(
        IMemberChapterServiceRequest request,
        string subject,
        string message,
        string recaptchaToken);

    /// <summary>
    /// Opens a thread with the site's admins. Takes no reCAPTCHA token, unlike its chapter counterpart: only
    /// a signed-in member reaches this, and what protects authed contact is a story of its own.
    /// </summary>
    Task<ServiceResult> StartSiteConversation(IMemberServiceRequest request, string subject, string message);

    Task<ServiceResult> UnarchiveChapterConversation(IMemberServiceRequest request, Guid conversationId);

    Task<ServiceResult> UnarchiveSiteConversation(IMemberServiceRequest request, Guid conversationId);
}
