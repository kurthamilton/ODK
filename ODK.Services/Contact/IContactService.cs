namespace ODK.Services.Contact;

public interface IContactService
{
    Task<ServiceResult> ReplyToChapterConversation(IMemberServiceRequest request, Guid conversationId, string message);

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
}