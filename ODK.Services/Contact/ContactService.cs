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
        var (currentMember, conversation) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterConversationRepository.GetById(conversationId));

        OdkAssertions.MeetsCondition(conversation, x => x.MemberId == request.CurrentMemberId);

        var (chapter, adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(conversation.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(conversation.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(
                conversation.ChapterId, NotificationType.ConversationOwnerMessage));

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversationId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = request.CurrentMemberId,
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
            request,
            chapter,
            conversation,
            conversationMessage,
            addressees.ToArray(),
            isReply: true);

        return ServiceResult.Successful();
    }

    public async Task SendChapterContactMessage(
        ServiceRequest request,
        Guid chapterId,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetById(chapterId).Run();
        await SendChapterContactMessage(request, chapter, fromAddress, message, recaptchaToken);
    }

    public async Task SendChapterContactMessage(
        ServiceRequest request,
        Chapter chapter,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        ValidateRequest(fromAddress, message);

        var result = await _recaptchaService.Verify(recaptchaToken);
        var flagged = !_recaptchaService.Success(result);
        if (flagged)
        {
            message = $"[FLAGGED AS SPAM: {result.Score} / 1.0] {message}";
        }

        var (adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
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
            await _memberEmailService.SendChapterMessage(request, chapter, adminMembers, contactMessage);
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
        MemberServiceRequest request,
        Guid chapterId,
        string subject,
        string message,
        string recaptchaToken)
    {
        var currentMemberId = request.CurrentMemberId;

        var (
            chapter,
            currentMember,
            memberSubscription,
            privacySettings,
            membershipSettings,
            adminMembers,
            notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapterId, NotificationType.ConversationOwnerMessage));

        if (!_authorizationService.CanStartConversation(chapterId, currentMember, memberSubscription, membershipSettings, privacySettings))
        {
            return ServiceResult.Failure("Permission denied");
        }

        var result = await _recaptchaService.Verify(recaptchaToken);

        var now = DateTime.UtcNow;

        var conversation = new ChapterConversation
        {
            ChapterId = chapterId,
            CreatedUtc = now,
            MemberId = currentMemberId,
            Subject = subject
        };

        _unitOfWork.ChapterConversationRepository.Add(conversation);

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversation.Id,
            CreatedUtc = now,
            MemberId = currentMemberId,
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
            request,
            chapter,
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
