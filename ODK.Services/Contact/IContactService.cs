using ODK.Core.Chapters;

namespace ODK.Services.Contact;

public interface IContactService
{
    Task SendChapterContactMessage(Chapter chapter, string fromAddress, string message, string recaptchaToken);

    Task SendSiteContactMessage(string fromAddress, string message, string recaptchaToken);
}
