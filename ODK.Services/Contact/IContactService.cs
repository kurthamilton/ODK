using ODK.Core.Chapters;
using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactService
{
    Task<ChapterContactPageViewModel> GetChapterContactPageViewModel(Guid currentMemberId, Guid chapterId);

    Task<ServiceResult> ReplyToChapterConversation(Guid currentMemberId, Guid conversationId, string message);

    Task SendChapterContactMessage(Guid chapterId, string fromAddress, string message, string recaptchaToken);

    Task SendChapterContactMessage(Chapter chapter, string fromAddress, string message, string recaptchaToken);

    Task SendSiteContactMessage(string fromAddress, string message, string recaptchaToken);

    Task<ServiceResult> StartChapterConversation(Guid currentMemberId, Guid chapterId, 
        string subject, string message, string recaptchaToken);
}
