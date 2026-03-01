using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Notifications.ViewModels;

namespace ODK.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void AddEventWaitlistPromotionNotifications(
        Event @event,
        IEnumerable<Member> members,
        IEnumerable<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.EventWaitlistPromotion,
            _ => $"Good news - a spot opened up and you're now attending {@event.GetDisplayName()}.",
            members,
            settings: settings,
            entityId: @event.Id,
            chapterId: @event.ChapterId);
    }

    public void AddNewChapterContactMessageNotifications(
        ChapterContactMessage message,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.ChapterContactMessage,
            _ => message.FromAddress,
            adminMembers.Select(x => x.Member),
            settings,
            entityId: message.Id,
            chapterId: message.ChapterId);
    }

    public void AddNewConversationAdminMessageNotifications(
        ChapterConversation conversation,
        Member member,
        IReadOnlyCollection<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.ConversationReplies,
            _ => conversation.Subject,
            [member],
            settings,
            entityId: conversation.Id,
            chapterId: conversation.ChapterId);
    }

    public void AddNewConversationOwnerMessageNotifications(
        ChapterConversation conversation,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.ConversationOwnerMessage,
            _ => conversation.Subject,
            adminMembers.Select(x => x.Member),
            settings,
            entityId: conversation.Id,
            chapterId: conversation.ChapterId);
    }

    public void AddNewEventNotifications(
        Chapter chapter,
        Event @event,
        Venue venue,
        IReadOnlyCollection<Member> members,
        IReadOnlyCollection<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.NewEvent,
            x => string.Join(Environment.NewLine,
                @event.Name,
                @event.Date.ToFriendlyDateTimeString(chapter.TimeZone ?? x.TimeZone),
                venue.Name),
            members.Where(x => x.IsCurrent()),
            settings,
            entityId: @event.Id,
            chapterId: @event.ChapterId,
            expiresUtc: @event.Date);
    }

    public void AddNewMemberNotifications(
        Member member,
        Guid chapterId,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings)
    {
        AddNotifications(
            NotificationType.NewMember,
            _ => member.FullName,
            adminMembers.Select(x => x.Member),
            settings,
            entityId: member.Id,
            chapterId: chapterId);
    }

    public async Task<NotificationsPageViewModel> GetNotificationsPageViewModel(
        IMemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var (settings, chapterSettings, adminChapters, memberChapters) = await _unitOfWork.RunAsync(
            x => x.MemberNotificationSettingsRepository.GetByMemberId(currentMember.Id),
            x => x.MemberChapterNotificationSettingsRepository.Query().ForMember(currentMember.Id).GetAll(),
            x => x.ChapterAdminMemberRepository.Query(platform).ForMember(currentMember.Id).ToDto().GetAll(),
            x => x.MemberChapterRepository.Query().ForMember(currentMember.Id).ToDto().GetAll());

        return new NotificationsPageViewModel
        {
            AdminChapters = adminChapters,
            ChapterSettings = chapterSettings,
            MemberChapters = memberChapters,
            Settings = settings
        };
    }

    public async Task<UnreadNotificationsViewModel> GetUnreadNotificationsViewModel(
        IMemberServiceRequest request)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var notifications = await _unitOfWork.NotificationRepository
            .GetUnreadDtosByMemberId(currentMember.Id)
            .Run();

        return new UnreadNotificationsViewModel
        {
            CurrentMember = currentMember,
            Platform = platform,
            Unread = notifications
        };
    }

    public async Task MarkAllAsRead(Guid memberId)
    {
        var unread = await _unitOfWork.NotificationRepository
            .GetUnreadByMemberId(memberId)
            .Run();

        var utcNow = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.ReadUtc = utcNow;
            _unitOfWork.NotificationRepository.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAsRead(Guid memberId, Guid notificationId)
    {
        var notification = await _unitOfWork.NotificationRepository
            .GetById(notificationId)
            .Run();

        if (notification.MemberId != memberId)
        {
            return;
        }

        notification.ReadUtc = DateTime.UtcNow;
        _unitOfWork.NotificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberNotificationSettings(
        Guid memberId,
        IReadOnlyCollection<NotificationType> disabledTypes)
    {
        var settings = await _unitOfWork.MemberNotificationSettingsRepository
            .GetByMemberId(memberId)
            .Run();

        var settingsDictionary = settings
            .ToDictionary(x => x.NotificationType);

        foreach (var type in disabledTypes)
        {
            settingsDictionary.TryGetValue(type, out var setting);

            if (setting == null)
            {
                _unitOfWork.MemberNotificationSettingsRepository.Add(new MemberNotificationSettings
                {
                    Disabled = true,
                    MemberId = memberId,
                    NotificationType = type
                });
            }
            else if (!setting.Disabled)
            {
                setting.Disabled = true;
                _unitOfWork.MemberNotificationSettingsRepository.Update(setting);
            }
        }

        foreach (var type in settingsDictionary.Keys)
        {
            if (disabledTypes.Contains(type))
            {
                continue;
            }

            var setting = settingsDictionary[type];
            _unitOfWork.MemberNotificationSettingsRepository.Delete(setting);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private void AddNotifications(
        NotificationType type,
        Func<Member, string> text,
        IEnumerable<Member> members,
        IEnumerable<MemberNotificationSettings> settings,
        Guid? entityId,
        Guid? chapterId,
        DateTime? expiresUtc = null)
    {
        var now = DateTime.UtcNow;

        var settingsDictionary = settings
            .Where(x => x.NotificationType == type)
            .ToDictionary(x => x.MemberId);

        foreach (var member in members)
        {
            settingsDictionary.TryGetValue(member.Id, out var memberSettings);
            if (memberSettings?.Disabled == true)
            {
                continue;
            }

            _unitOfWork.NotificationRepository.Add(new Notification
            {
                ChapterId = chapterId,
                CreatedUtc = now,
                EntityId = entityId,
                ExpiresUtc = expiresUtc,
                MemberId = member.Id,
                Text = text(member),
                Type = type
            });
        }
    }
}