using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Contact.ViewModels;
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
    private readonly IPlatformProvider _platformProvider;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;
    
    public ContactService(
        IRecaptchaService recaptchaService,
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider,
        IAuthorizationService authorizationService,
        INotificationService notificationService,
        IMemberEmailService memberEmailService)
    {
        _authorizationService = authorizationService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _platformProvider = platformProvider;
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChapterContactPageViewModel> GetChapterContactPageViewModel(Guid currentMemberId, Guid chapterId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, currentMember, memberSubscription, membershipSettings, privacySettings, conversations) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterConversationRepository.GetByMemberId(currentMemberId, chapterId));

        var canStartConversation = _authorizationService.CanStartConversation(chapterId, currentMember, memberSubscription,
            membershipSettings, privacySettings);

        return new ChapterContactPageViewModel
        {
            CanStartConversation = canStartConversation,
            Chapter = chapter,
            Conversations = conversations,
            Platform = platform
        };
    }

    public async Task<ServiceResult> ReplyToChapterConversation(Guid currentMemberId, Guid conversationId, string message)
    {
        var (currentMember, conversation) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterConversationRepository.GetById(conversationId));

        OdkAssertions.MeetsCondition(conversation, x => x.MemberId == currentMemberId);

        var (chapter, adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(conversation.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(conversation.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(conversation.ChapterId, NotificationType.ConversationOwnerMessage));

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversationId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = currentMemberId,
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
            chapter, 
            conversation, 
            conversationMessage, 
            addressees.ToArray(), 
            isReply: true);

        return ServiceResult.Successful();
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

        var (adminMembers, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.ChapterContactMessage));

        var contactMessage = new ChapterContactMessage
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = recaptchaResponse.Score
        };

        _unitOfWork.ChapterContactMessageRepository.Add(contactMessage);

        _notificationService.AddNewChapterContactMessageNotifications(
            contactMessage,
            adminMembers,
            notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendChapterMessage(chapter, adminMembers, contactMessage);
    }

    public async Task SendSiteContactMessage(string fromAddress, string message, string recaptchaToken)
    {
        ValidateRequest(fromAddress, message);

        var platform = _platformProvider.GetPlatform();

        var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();

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

        await _memberEmailService.SendSiteMessage(contactMessage, siteEmailSettings);
    }

    public async Task<ServiceResult> StartChapterConversation(Guid currentMemberId, Guid chapterId, 
        string subject, string message, string recaptchaToken)
    {
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

        var recaptchaResponse = await _recaptchaService.Verify(recaptchaToken);        

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
            RecaptchaScore = recaptchaResponse.Score,
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
