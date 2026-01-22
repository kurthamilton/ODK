using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new Regex("http://|https://|<script>.*</script>|<img",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberEmailService memberEmailService,
        IPaymentProviderFactory paymentProviderFactory,
        ILoggingService loggingService)
    {
        _authorizationService = authorizationService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddComment(MemberServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId)
    {
        var currentMemberId = request.CurrentMemberId;

        var (member, @event) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId));

        var (chapter, settings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(@event.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(@event.ChapterId));

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

    public async Task<(Chapter, Event)> GetEvent(Guid eventId)
    {
        var @event = await _unitOfWork.EventRepository.GetById(eventId).Run();
        var chapter = await _unitOfWork.ChapterRepository.GetById(@event.ChapterId).Run();
        return (chapter, @event);
    }

    public async Task<ServiceResult> UpdateMemberResponse(Guid memberId, Guid eventId,
        EventResponseType responseType)
    {
        responseType = NormalizeResponseType(responseType);

        var (member, @event, memberResponse, numberOfAttendees) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId),
            x => x.EventResponseRepository.GetNumberOfAttendees(eventId));

        if (@event.RsvpDisabled)
        {
            return ServiceResult.Failure("RSVP is currently disabled");
        }

        if (memberResponse?.Type == responseType)
        {
            return ServiceResult.Successful();
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

    private async Task<ServiceResult<Payment>> MakeEventPayment(
        Event @event,
        Member member,
        decimal amount,
        string cardToken,
        SitePaymentSettings sitePaymentSettings,
        ChapterPaymentAccount? chapterPaymentAccount)
    {
        if (@event.TicketSettings == null)
        {
            return ServiceResult<Payment>.Failure("Event is not ticketed");
        }

        var (chapterId, currency) = (@event.ChapterId, @event.TicketSettings.Currency);

        var payment = new Payment
        {
            Amount = amount,
            ChapterId = @event.ChapterId,
            CurrencyId = currency.Id,
            ExternalId = string.Empty,
            MemberId = member.Id,
            Reference = @event.Name
        };

        _unitOfWork.PaymentRepository.Add(payment);
        await _unitOfWork.SaveChangesAsync();

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings,
            chapterPaymentAccount);

        var paymentResult = await paymentProvider.MakePayment(
            currency.Code,
            amount,
            cardToken,
            @event.Name,
            member.Id,
            member.FullName);
        if (!paymentResult.Success)
        {
            return ServiceResult<Payment>.Failure($"Payment not made: {paymentResult.Message}");
        }

        payment.ExternalId = paymentResult.Id;
        payment.PaidUtc = DateTime.UtcNow;
        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Payment>.Successful(payment);
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