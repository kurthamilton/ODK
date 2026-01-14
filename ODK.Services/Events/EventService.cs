using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
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

    public async Task<ServiceResult> PayDeposit(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, memberResponse, @event, numberOfAttendees, payments) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventResponseRepository.GetByMemberId(currentMemberId, eventId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetNumberOfAttendees(eventId),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(currentMemberId, eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        if (@event.TicketSettings.Deposit == null)
        {
            return ServiceResult.Failure("This event does not require a deposit");
        }

        var paidSoFar = payments.Sum(x => x.Payment.Amount);
        if (paidSoFar > @event.TicketSettings.Deposit.Value)
        {
            return ServiceResult.Failure("You have already paid a deposit for this event");
        }

        var chapterId = @event.ChapterId;

        var (
            sitePaymentSettings,
            chapterPaymentSettings,
            chapterPaymentAccount,
            ownerSubscription,
            membershipSettings,
            privacySettings,
            memberSubscription) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) != true)
        {
            return ServiceResult.Failure("Payment not made: this group can no longer receive payments");
        }

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

        var spacesLeft = EventHasSpaces(@event, numberOfAttendees);
        if (!spacesLeft.Success)
        {
            return spacesLeft;
        }

        var paymentResult = await MakeEventPayment(
            @event,
            member,
            @event.TicketSettings.Deposit.Value,
            cardToken,
            chapterPaymentSettings,
            sitePaymentSettings,
            chapterPaymentAccount);

        if (paymentResult.Value == null)
        {
            return paymentResult;
        }

        _unitOfWork.EventResponseRepository.Add(new EventResponse
        {
            EventId = eventId,
            MemberId = member.Id,
            Type = EventResponseType.Yes
        });

        _unitOfWork.EventTicketPaymentRepository.Add(new EventTicketPayment
        {
            EventId = eventId,
            PaymentId = paymentResult.Value.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PayTicketRemainder(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, @event, payments) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(currentMemberId, eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        var paidSoFar = payments.Sum(x => x.Payment.Amount);

        if (paidSoFar >= @event.TicketSettings.Cost)
        {
            return ServiceResult.Failure("You have already bought a ticket");
        }

        var (sitePaymentSettings, chapterPaymentSettings, chapterPaymentAccount) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(@event.ChapterId));

        var amount = @event.TicketSettings.Cost - paidSoFar;
        var paymentResult = await MakeEventPayment(
            @event,
            member,
            amount,
            cardToken,
            chapterPaymentSettings,
            sitePaymentSettings,
            chapterPaymentAccount);
        if (paymentResult.Value == null)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        _unitOfWork.EventTicketPaymentRepository.Add(new EventTicketPayment
        {
            EventId = eventId,
            PaymentId = paymentResult.Value.Id
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PurchaseTicket(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, memberResponse, @event, payments, numberOfAttendees) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventResponseRepository.GetByMemberId(currentMemberId, eventId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(currentMemberId, eventId),
            x => x.EventResponseRepository.GetNumberOfAttendees(eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        var paidSoFar = payments.Sum(x => x.Payment.Amount);

        if (paidSoFar >= @event.TicketSettings.Cost)
        {
            return ServiceResult.Failure("You have already purchased a ticket for this event");
        }

        var (
            sitePaymentSettings,
            chapterPaymentSettings,
            chapterPaymentAccount,
            ownerSubscription,
            membershipSettings,
            privacySettings,
            memberSubscription) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, @event.ChapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) != true)
        {
            return ServiceResult.Failure("Payment not made: this group can no longer receive payments");
        }

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

        var spacesLeft = EventHasSpaces(@event, numberOfAttendees);
        if (!spacesLeft.Success)
        {
            return spacesLeft;
        }

        var paymentResult = await MakeEventPayment(
            @event,
            member,
            amount: @event.TicketSettings.Cost,
            cardToken,
            chapterPaymentSettings,
            sitePaymentSettings,
            chapterPaymentAccount);
        if (paymentResult.Value == null)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        _unitOfWork.EventResponseRepository.Add(new EventResponse
        {
            EventId = eventId,
            MemberId = member.Id,
            Type = EventResponseType.Yes
        });

        _unitOfWork.EventTicketPaymentRepository.Add(new EventTicketPayment
        {
            EventId = eventId,
            PaymentId = paymentResult.Value.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
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
        ChapterPaymentSettings? chapterPaymentSettings,
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
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
            chapterPaymentSettings,
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