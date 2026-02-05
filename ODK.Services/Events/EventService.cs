using System.Text.RegularExpressions;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Tasks;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new(
        "http://|https://|<script>.*</script>|<img",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService,
        IBackgroundTaskService backgroundTaskService,
        INotificationService notificationService)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddComment(
        IMemberChapterServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (@event, settings) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (settings?.DisableComments == true || !@event.CanComment || !@event.IsAuthorized(currentMember))
        {
            return ServiceResult.Failure("You cannot comment on this event");
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return ServiceResult.Failure("Comment required");
        }

        EventComment? parentComment = null;
        Member? parentCommentMember = null;
        if (parentEventCommentId != null)
        {
            parentComment = await _unitOfWork.EventCommentRepository.GetByIdOrDefault(parentEventCommentId.Value).Run();
            if (parentComment != null && parentComment.EventId != @event.Id)
            {
                parentComment = null;
            }

            if (parentComment != null)
            {
                parentCommentMember = await _unitOfWork.MemberRepository.GetById(parentComment.MemberId).Run();
            }
        }

        var hidden = HideCommentRegex.IsMatch(comment);

        var eventComment = new EventComment
        {
            CreatedUtc = DateTime.UtcNow,
            EventId = eventId,
            Hidden = hidden,
            MemberId = currentMember.Id,
            ParentEventCommentId = parentComment?.Id,
            Text = comment
        };
        _unitOfWork.EventCommentRepository.Add(eventComment);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendEventCommentEmail(
            request,
            @event,
            eventComment,
            parentCommentMember);

        return ServiceResult.Successful();
    }

    public async Task CompleteEventTicketPurchase(Guid eventId, Guid memberId)
    {
        var (@event, response, payments) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(memberId, eventId));

        if (@event.TicketSettings == null)
        {
            await _loggingService.Warn(
                $"Cannot complete event ticket purchase for event {eventId} " +
                $"for member {memberId}: event is not ticketed");
            return;
        }

        var amountPaid = payments.Sum(x => x.Payment.Amount);
        if (amountPaid < @event.TicketSettings.Cost)
        {
            await _loggingService.Info($"Not completing event ticket purchase for event {eventId} " +
                $"for member {memberId}: {amountPaid} has been paid of the full amount {@event.TicketSettings.Cost}");
            return;
        }

        if (response?.Type == EventResponseType.Yes)
        {
            return;
        }

        response ??= new EventResponse
        {
            MemberId = memberId
        };

        response.Type = EventResponseType.Yes;

        if (response.EventId == default)
        {
            response.EventId = eventId;
            _unitOfWork.EventResponseRepository.Add(response);
        }
        else
        {
            _unitOfWork.EventResponseRepository.Update(response);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public Task<Event> GetById(Guid eventId) => _unitOfWork.EventRepository.GetById(eventId).Run();

    public async Task<ServiceResult> JoinWaitlist(Guid eventId, Guid memberId)
    {
        var (@event, member, isOnWaitlist) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(memberId),
            x => x.EventWaitlistMemberRepository.IsOnWaitlist(memberId, eventId));

        if (isOnWaitlist)
        {
            return ServiceResult.Successful();
        }

        var authorisationResult = GetMemberEventAuthorisation(@event, member, isForAdmin: false);
        if (!authorisationResult.Success)
        {
            return authorisationResult;
        }

        _unitOfWork.EventWaitlistMemberRepository.Add(new EventWaitlistMember
        {
            CreatedUtc = DateTime.UtcNow,
            EventId = eventId,
            MemberId = memberId
        });

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> LeaveWaitlist(Guid eventId, Guid memberId)
    {
        var (@event, member, waitlistMember) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(memberId),
            x => x.EventWaitlistMemberRepository.GetByMemberId(memberId, eventId));

        if (waitlistMember == null)
        {
            return ServiceResult.Successful("You have left the waiting list");
        }

        _unitOfWork.EventWaitlistMemberRepository.Delete(waitlistMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful("You have left the waiting list");
    }

    public async Task NotifyWaitlist(IServiceRequest request, Guid eventId)
    {
        var platform = request.Platform;

        var (chapter, @event, waitlist, responses) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByEventId(platform, eventId),
            x => x.EventRepository.GetById(@eventId),
            x => x.EventWaitlistMemberRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId));

        if (@event.Ticketed)
        {
            // do not auto-promote waitlist for ticketed events
            return;
        }

        if (@event.RsvpDeadlinePassed)
        {
            return;
        }

        if (waitlist.Count == 0)
        {
            return;
        }

        var membersToConfirm = new List<EventWaitlistMember>();

        var numberOfAttendees = responses.Count(x => x.Type == EventResponseType.Yes);

        var spacesLeft = @event.NumberOfSpacesLeft(numberOfAttendees);
        if (spacesLeft == null)
        {
            spacesLeft = waitlist.Count;
        }

        if (spacesLeft <= 0)
        {
            return;
        }

        var responseDictionary = responses.ToDictionary(x => x.MemberId);

        var waitlistToPromote = waitlist
            .OrderBy(x => x.CreatedUtc)
            .Take(spacesLeft.Value)
            .ToArray();

        membersToConfirm.AddRange(waitlistToPromote);

        _unitOfWork.EventWaitlistMemberRepository.DeleteMany(waitlistToPromote);

        foreach (var waitlistMember in waitlistToPromote)
        {
            if (responseDictionary.TryGetValue(waitlistMember.MemberId, out var response))
            {
                response.Type = EventResponseType.Yes;
                _unitOfWork.EventResponseRepository.Update(response);
            }
            else
            {
                _unitOfWork.EventResponseRepository.Add(new EventResponse
                {
                    EventId = eventId,
                    MemberId = waitlistMember.MemberId,
                    Type = EventResponseType.Yes
                });
            }
        }

        var memberIds = membersToConfirm
            .Select(x => x.MemberId)
            .ToArray();

        var (members, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByIds(memberIds),
            x => x.MemberNotificationSettingsRepository.GetByMemberIds(memberIds, NotificationType.EventWaitlistPromotion));

        _notificationService.AddEventWaitlistPromotionNotifications(@event, members, notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendEventWaitlistPromotionNotification(
            ChapterServiceRequest.Create(chapter, request), @event, members);
    }

    public async Task<ServiceResult> UpdateMemberResponse(
        IMemberServiceRequest request,
        Guid eventId,
        EventResponseType responseType,
        Guid? adminMemberId)
    {
        var currentMember = request.CurrentMember;

        var (@event, memberResponse, waitlist) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(currentMember.Id, eventId),
            x => x.EventWaitlistMemberRepository.GetByEventId(eventId));

        return await UpdateMemberResponse(
            request,
            @event,
            responseType,
            memberResponse,
            waitlist,
            adminMemberId);
    }

    public async Task<ServiceResult> UpdateMemberResponse(
        IMemberServiceRequest request, string shortcode, EventResponseType responseType, Guid? adminMemberId)
    {
        var currentMember = request.CurrentMember;

        var (@event, memberResponse, waitlist) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetByShortcode(shortcode),
            x => x.EventResponseRepository.GetByMemberId(currentMember.Id, shortcode),
            x => x.EventWaitlistMemberRepository.GetByEventShortcode(shortcode));

        return await UpdateMemberResponse(
            request,
            @event,
            responseType,
            memberResponse,
            waitlist,
            adminMemberId);
    }

    private static EventResponseType NormalizeResponseType(EventResponseType responseType)
    {
        var validResponseTypes = new[]
        {
            EventResponseType.Yes,
            EventResponseType.No,
            EventResponseType.Maybe,
            EventResponseType.None
        };

        return validResponseTypes.Contains(responseType)
            ? responseType
            : EventResponseType.None;
    }

    private static ServiceResult EventHasSpaces(
        Event @event,
        int numberOfAttendees)
    {
        if (@event.NumberOfSpacesLeft(numberOfAttendees) <= 0)
        {
            return ServiceResult.Failure("No more spaces left");
        }

        return ServiceResult.Successful();
    }

    private static ServiceResult GetMemberEventAuthorisation(Event @event, Member member, bool isForAdmin)
    {
        if (@event.RsvpDisabled)
        {
            return ServiceResult.Failure("RSVP is currently disabled");
        }

        if (!@event.IsAuthorized(member))
        {
            var message = isForAdmin
                ? "Member is not permitted to attend"
                : "You are not permitted to attend";
            return ServiceResult.Failure(message);
        }

        return ServiceResult.Successful();
    }

    private ServiceResult MemberCanAttendEvent(
        Event @event,
        Member? member,
        EventResponse? memberResponse,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings,
        bool isForAdmin)
    {
        if (@event.Date < DateTime.Today)
        {
            return ServiceResult.Failure("Past events cannot be responded to");
        }

        var canRespond = _authorizationService.CanRespondToEvent(
            @event, member, subscription, membershipSettings, privacySettings);
        if (canRespond)
        {
            return ServiceResult.Successful();
        }

        return isForAdmin
            ? ServiceResult.Failure("Member is not permitted to attend this event")
            : ServiceResult.Failure("You are not permitted to attend this event");
    }

    private async Task<ServiceResult> UpdateMemberResponse(
        IMemberServiceRequest request,
        Event @event,
        EventResponseType responseType,
        EventResponse? memberResponse,
        IReadOnlyCollection<EventWaitlistMember> waitlist,
        Guid? adminMemberId)
    {
        var (eventId, member) = (@event.Id, request.CurrentMember);

        responseType = NormalizeResponseType(responseType);
        if (memberResponse?.Type == responseType)
        {
            return ServiceResult.Successful();
        }

        var authorisationResult = GetMemberEventAuthorisation(
            @event, member, isForAdmin: adminMemberId != null);
        if (!authorisationResult.Success)
        {
            return authorisationResult;
        }

        if (@event.TicketSettings != null)
        {
            return ServiceResult.Failure("Ticketed events cannot be responded to");
        }

        var (membershipSettings, privacySettings, memberSubscription, numberOfAttendees) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(member.Id, @event.ChapterId),
            x => x.EventResponseRepository.GetNumberOfAttendees(eventId));

        var validationResult = MemberCanAttendEvent(
            @event,
            member,
            memberResponse,
            memberSubscription,
            membershipSettings,
            privacySettings,
            isForAdmin: adminMemberId != null);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (responseType == EventResponseType.Yes)
        {
            var spacesLeft = EventHasSpaces(@event, numberOfAttendees);
            if (!spacesLeft.Success)
            {
                if (!@event.WaitlistDisabled && !waitlist.Any(x => x.MemberId == member.Id))
                {
                    _unitOfWork.EventWaitlistMemberRepository.Add(new EventWaitlistMember
                    {
                        CreatedUtc = DateTime.UtcNow,
                        EventId = eventId,
                        MemberId = member.Id
                    });
                    await _unitOfWork.SaveChangesAsync();

                    var message = adminMemberId != null
                        ? "No more spaces left. Member has been added to the waiting list"
                        : "No more spaces left. You have been added to the waiting list";
                    return ServiceResult.Failure(message);
                }

                return spacesLeft;
            }
        }

        if (@event.RsvpDeadlinePassed)
        {
            if (memberResponse?.Type != EventResponseType.Yes)
            {
                return ServiceResult.Failure("The RSVP deadline has passed");
            }
        }

        if (responseType == EventResponseType.None)
        {
            if (memberResponse != null)
            {
                _unitOfWork.EventResponseRepository.Delete(memberResponse);
            }
        }
        else if (memberResponse == null)
        {
            _unitOfWork.EventResponseRepository.Add(new EventResponse
            {
                EventId = eventId,
                MemberId = member.Id,
                Type = responseType
            });
        }
        else
        {
            memberResponse.Type = responseType;
            _unitOfWork.EventResponseRepository.Update(memberResponse);
        }

        await _unitOfWork.SaveChangesAsync();

        _backgroundTaskService.Enqueue(() => NotifyWaitlist(request, eventId));

        return ServiceResult.Successful();
    }
}