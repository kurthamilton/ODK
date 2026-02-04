namespace ODK.Services.Contact;

public interface IContactService
{
    Task<ServiceResult> ReplyToChapterConversation(MemberServiceRequest request, Guid conversationId, string message);

    Task SendChapterContactMessage(
        ChapterServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken);

    Task SendSiteContactMessage(
        ServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken);

    Task<ServiceResult> StartChapterConversation(
        MemberChapterServiceRequest request,
        string subject,
        string message,
        string recaptchaToken);
}
