using System.Text.RegularExpressions;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Tasks;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new Regex("http://|https://|<script>.*</script>|<img",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService,
        IBackgroundTaskService backgroundTaskService)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddComment(
        MemberServiceRequest request, Guid eventId, Chapter chapter, string comment, Guid? parentEventCommentId)
    {
        var currentMemberId = request.CurrentMemberId;

        var (member, @event, settings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (settings?.DisableComments == true || !@event.CanComment || !@event.IsAuthorized(member))
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
            MemberId = currentMemberId,
            ParentEventCommentId = parentComment?.Id,
            Text = comment
        };
        _unitOfWork.EventCommentRepository.Add(eventComment);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendEventCommentEmail(
            request,
            chapter,
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

    public async Task<ServiceResult> JoinWaitingList(Guid eventId, Guid memberId)
    {
        var (@event, member, isOnWaitingList) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(memberId),
            x => x.EventWaitingListMemberRepository.IsOnWaitingList(memberId, eventId));

        if (isOnWaitingList)
        {
            return ServiceResult.Successful();
        }

        var authorisationResult = GetMemberEventAuthorisation(@event, member);
        if (!authorisationResult.Success)
        {
            return authorisationResult;
        }

        _unitOfWork.EventWaitingListMemberRepository.Add(new EventWaitingListMember
        {
            CreatedUtc = DateTime.UtcNow,
            EventId = eventId,
            MemberId = memberId
        });

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> LeaveWaitingList(Guid eventId, Guid memberId)
    {
        var (@event, member, waitingListMember) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(memberId),
            x => x.EventWaitingListMemberRepository.GetByMemberId(memberId, eventId));

        if (waitingListMember == null)
        {
            return ServiceResult.Successful("You have left the waiting list");
        }

        _unitOfWork.EventWaitingListMemberRepository.Delete(waitingListMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful("You have left the waiting list");
    }

    public async Task NotifyWaitingList(ServiceRequest request, Guid eventId)
    {
        var (@event, waitingList, attendees) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(@eventId),
            x => x.EventWaitingListMemberRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId, EventResponseType.Yes));

        if (@event.RsvpDeadlinePassed)
        {
            return;
        }

        if (waitingList.Count == 0)
        {
            return;
        }

        var membersToConfirm = new List<EventWaitingListMember>();

        var spacesLeft = @event.NumberOfSpacesLeft(attendees.Count);
        if (spacesLeft == null)
        {
            spacesLeft = waitingList.Count;
        }

        if (spacesLeft <= 0)
        {
            return;
        }

        var attendeeDictionary = attendees.ToDictionary(x => x.MemberId);

        var waitingListToPromote = waitingList
            .OrderBy(x => x.CreatedUtc)
            .Take(spacesLeft.Value)
            .ToArray();

        membersToConfirm.AddRange(waitingList);

        if (@event.AttendeeLimit == null)
        {
        }
        else
        {
        }
    }

    public async Task<ServiceResult> UpdateMemberResponse(
        MemberServiceRequest request,
        Guid eventId,
        EventResponseType responseType)
    {
        var memberId = request.CurrentMemberId;
        responseType = NormalizeResponseType(responseType);

        var (member, @event, memberResponse, numberOfAttendees, waitingList) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId),
            x => x.EventResponseRepository.GetNumberOfAttendees(eventId),
            x => x.EventWaitingListMemberRepository.GetByEventId(eventId));

        if (memberResponse?.Type == responseType)
        {
            return ServiceResult.Successful();
        }

        var authorisationResult = GetMemberEventAuthorisation(@event, member);
        if (!authorisationResult.Success)
        {
            return authorisationResult;
        }

        if (@event.TicketSettings != null)
        {
            return ServiceResult.Failure("Ticketed events cannot be responded to");
        }

        var (membershipSettings, privacySettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, @event.ChapterId));

        var validationResult = MemberCanAttendEvent(
            @event,
            member,
            memberResponse,
            memberSubscription,
            membershipSettings,
            privacySettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (responseType == EventResponseType.Yes)
        {
            var spacesLeft = EventHasSpaces(@event, numberOfAttendees);
            if (!spacesLeft.Success)
            {
                if (!@event.WaitingListDisabled && !waitingList.Any(x => x.MemberId == memberId))
                {
                    _unitOfWork.EventWaitingListMemberRepository.Add(new EventWaitingListMember
                    {
                        CreatedUtc = DateTime.UtcNow,
                        EventId = eventId,
                        MemberId = memberId
                    });
                    await _unitOfWork.SaveChangesAsync();

                    return ServiceResult.Failure("No more spaces left. You have been added to the waiting list");
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

        _backgroundTaskService.Enqueue(() => NotifyWaitingList(request, eventId));

        return ServiceResult.Successful();
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

    private static ServiceResult GetMemberEventAuthorisation(Event @event, Member member)
    {
        if (@event.RsvpDisabled)
        {
            return ServiceResult.Failure("RSVP is currently disabled");
        }

        if (!@event.IsAuthorized(member))
        {
            return ServiceResult.Failure("You are not permitted to attend");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult MemberCanAttendEvent(
        Event @event,
        Member? member,
        EventResponse? memberResponse,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        if (@event.Date < DateTime.Today)
        {
            return ServiceResult.Failure("Past events cannot be responded to");
        }

        return _authorizationService.CanRespondToEvent(@event, member, subscription, membershipSettings, privacySettings)
            ? ServiceResult.Successful()
            : ServiceResult.Failure("You are not permitted to attend this event");
    }
}