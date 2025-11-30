using ODK.Core.Chapters;
using ODK.Core.Web;
using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactService
{
    Task<ServiceResult> ReplyToChapterConversation(MemberServiceRequest request, Guid conversationId, string message);

    Task SendChapterContactMessage(
        ServiceRequest request, 
        Guid chapterId, 
        string fromAddress, 
        string message, 
        string recaptchaToken);

    Task SendChapterContactMessage(
        ServiceRequest request, 
        Chapter chapter, 
        string fromAddress, 
        string message, 
        string recaptchaToken);

    Task SendSiteContactMessage(
        ServiceRequest request, 
        string fromAddress, 
        string message, 
        string recaptchaToken);

    Task<ServiceResult> StartChapterConversation(
        MemberServiceRequest request, 
        Guid chapterId, 
        string subject, 
        string message, 
        string recaptchaToken);
}
