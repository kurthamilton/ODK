using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Recaptcha;

namespace ODK.Services.Contact;

public class ContactService : IContactService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(
        IRecaptchaService recaptchaService,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        INotificationService notificationService,
        IMemberEmailService memberEmailService)
    {
        _authorizationService = authorizationService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ReplyToChapterConversation(
        MemberServiceRequest request, Guid conversationId, string message)
    {
        var platform = request.Platform;

        var conversation = await _unitOfWork.ChapterConversationRepository.GetById(conversationId).Run();

        OdkAssertions.MeetsCondition(conversation, x => x.MemberId == request.CurrentMember.Id);

        var (chapter, adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(platform, conversation.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, conversation.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(
                conversation.ChapterId, NotificationType.ConversationOwnerMessage));

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversationId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = request.CurrentMember.Id,
            ReadByMember = true,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationOwnerMessageNotifications(
            conversation,
            adminMembers,
            notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        var addressees = adminMembers
            .Where(x => x.ReceiveContactEmails)
            .Select(x => x.Member);

        await _memberEmailService.SendChapterConversationEmail(
            ChapterServiceRequest.Create(chapter, request),
            conversation,
            conversationMessage,
            addressees.ToArray(),
            isReply: true);

        return ServiceResult.Successful();
    }

    public async Task SendChapterContactMessage(
        ChapterServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        ValidateRequest(fromAddress, message);

        var result = await _recaptchaService.Verify(recaptchaToken);
        var flagged = !_recaptchaService.Success(result);
        if (flagged)
        {
            message = $"[FLAGGED AS SPAM: {result.Score} / 1.0] {message}";
        }

        var (adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.ChapterContactMessage));

        var contactMessage = new ChapterContactMessage
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = result.Score
        };

        _unitOfWork.ChapterContactMessageRepository.Add(contactMessage);

        if (!flagged)
        {
            _notificationService.AddNewChapterContactMessageNotifications(
            contactMessage,
            adminMembers,
            notificationSettings);
        }

        await _unitOfWork.SaveChangesAsync();

        if (!flagged)
        {
            await _memberEmailService.SendChapterMessage(request, adminMembers, contactMessage);
        }
    }

    public async Task SendSiteContactMessage(
        ServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        ValidateRequest(fromAddress, message);

        var platform = request.Platform;

        var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();

        var result = await _recaptchaService.Verify(recaptchaToken);
        if (!_recaptchaService.Success(result))
        {
            message = $"[FLAGGED AS SPAM: {result.Score} / 1.0] {message}";
        }

        var contactMessage = new SiteContactMessage
        {
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = result.Score
        };

        _unitOfWork.SiteContactMessageRepository.Add(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendSiteMessage(request, contactMessage, siteEmailSettings);
    }

    public async Task<ServiceResult> StartChapterConversation(
        MemberChapterServiceRequest request,
        string subject,
        string message,
        string recaptchaToken)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            memberSubscription,
            privacySettings,
            membershipSettings,
            adminMembers,
            notificationSettings) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.ConversationOwnerMessage));

        if (!_authorizationService.CanStartConversation(chapter.Id, currentMember, memberSubscription, membershipSettings, privacySettings))
        {
            return ServiceResult.Failure("Permission denied");
        }

        var result = await _recaptchaService.Verify(recaptchaToken);

        var now = DateTime.UtcNow;

        var conversation = new ChapterConversation
        {
            ChapterId = chapter.Id,
            CreatedUtc = now,
            MemberId = currentMember.Id,
            Subject = subject
        };

        _unitOfWork.ChapterConversationRepository.Add(conversation);

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversation.Id,
            CreatedUtc = now,
            MemberId = currentMember.Id,
            ReadByMember = true,
            RecaptchaScore = result.Score,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationOwnerMessageNotifications(
            conversation,
            adminMembers,
            notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        var emailMembers = adminMembers
            .Where(x => x.ReceiveContactEmails)
            .Select(x => x.Member);

        await _memberEmailService.SendChapterConversationEmail(
            ChapterServiceRequest.Create(chapter, request),
            conversation,
            conversationMessage,
            emailMembers.ToArray(),
            isReply: false);

        return ServiceResult.Successful();
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
