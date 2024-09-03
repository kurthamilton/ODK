using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Messages;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Recaptcha;

namespace ODK.Services.Contact;

public class ContactService : IContactService
{
    private readonly IEmailService _emailService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(
        IRecaptchaService recaptchaService,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _emailService = emailService;
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public async Task SendChapterContactMessage(Guid chapterId, string fromAddress, string message, string recaptchaToken)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetById(chapterId).Run();
        await SendChapterContactMessage(chapter, fromAddress, message, recaptchaToken);
    }

    public async Task SendChapterContactMessage(Chapter chapter, string fromAddress, string message, string recaptchaToken)
    {
        ValidateRequest(fromAddress, message);

        var recaptchaResponse = await _recaptchaService.Verify(recaptchaToken);
        if (!_recaptchaService.Success(recaptchaResponse))
        {
            message = $"[FLAGGED AS SPAM: {recaptchaResponse.Score} / 1.0] {message}";
        }

        var contactMessage = new ChapterContactMessage
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = recaptchaResponse.Score
        };

        _unitOfWork.ChapterContactMessageRepository.Add(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendContactEmail(chapter, contactMessage);
    }

    public async Task SendSiteContactMessage(string fromAddress, string message, string recaptchaToken)
    {
        ValidateRequest(fromAddress, message);

        var recaptchaResponse = await _recaptchaService.Verify(recaptchaToken);
        if (!_recaptchaService.Success(recaptchaResponse))
        {
            message = $"[FLAGGED AS SPAM: {recaptchaResponse.Score} / 1.0] {message}";
        }

        var contactMessage = new SiteContactMessage
        {
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = recaptchaResponse.Score
        };

        _unitOfWork.SiteContactMessageRepository.Add(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendContactEmail(contactMessage);
    }

    private static void ValidateRequest(string fromAddress, string message)
    {
        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
        {
            throw new OdkServiceException("Email address and message must be provided");
        }

        if (!MailUtils.ValidEmailAddress(fromAddress))
        {
            throw new OdkServiceException("Invalid email address format");
        }
    }
}
