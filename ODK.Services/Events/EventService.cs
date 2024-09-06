using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Services.Payments;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new Regex("http://|https://|<script>.*</script>|<img", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public EventService(IUnitOfWork unitOfWork,       
        IAuthorizationService authorizationService,
        IEmailService emailService,
        IChapterUrlService chapterUrlService,
        IPaymentService paymentService,
        IUrlProvider urlProvider)
    {
        _authorizationService = authorizationService;
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task<ServiceResult> AddComment(Guid currentMemberId, Guid eventId, string comment, Guid? parentEventCommentId)
    {
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

        var parameters = new Dictionary<string, string>
        {
            { "comment.text", eventComment.Text },
            { "event.id", @event.Id.ToString() }
        };

        var url = _urlProvider.EventUrl(chapter, @event.Id);
        parameters.Add("event.url", url);

        await _emailService.SendEventCommentEmail(chapter, parentCommentMember, eventComment, parameters);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PayDeposit(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, @event, ticketPurchase) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),           
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPurchaseRepository.GetByMemberId(currentMemberId, eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        if (ticketPurchase?.DepositPurchasedUtc != null)
        {
            return ServiceResult.Failure("You have already paid a deposit for this event");
        }

        if (@event.TicketSettings.Deposit == null)
        {
            return ServiceResult.Failure("This event does not have deposits");
        }

        var (paymentSettings, ownerSubscription, membershipSettings, privacySettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, @event.ChapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) != null)
        {
            return ServiceResult.Failure("Payment not made: this group can no longer receive payments");
        }

        if (paymentSettings == null)
        {
            return ServiceResult.Failure("Payment not made: payments not configured");
        }

        var validationResult = MemberCanAttendEvent(@event, member, memberSubscription, membershipSettings, privacySettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var paymentResult = await _paymentService.MakePayment(paymentSettings, 
            paymentSettings.Currency, member, @event.TicketSettings.Deposit.Value, cardToken, @event.Name);
        if (!paymentResult.Success)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        _unitOfWork.EventResponseRepository.Add(new EventResponse
        {
            EventId = eventId,
            MemberId = member.Id,
            Type = EventResponseType.Yes
        });

        _unitOfWork.EventTicketPurchaseRepository.Add(new EventTicketPurchase
        {
            DepositPaid = @event.TicketSettings.Deposit,
            DepositPurchasedUtc = DateTime.UtcNow,
            EventId = eventId,
            MemberId = member.Id,
            TotalPaid = @event.TicketSettings.Deposit.Value
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PayTicketRemainder(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, @event, ticketPurchase) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPurchaseRepository.GetByMemberId(currentMemberId, eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        if (ticketPurchase == null || ticketPurchase.DepositPaid == null)
        {
            return ServiceResult.Failure("You have not paid a deposit for this event");
        }

        var paymentSettings = await _unitOfWork.ChapterPaymentSettingsRepository.GetByChapterId(@event.ChapterId).Run();
        if (paymentSettings == null)
        {
            return ServiceResult.Failure("Payment not made: payments not configured");
        }

        var amount = @event.TicketSettings.Cost - ticketPurchase.DepositPaid.Value;
        var paymentResult = await _paymentService.MakePayment(paymentSettings, 
            paymentSettings.Currency, member, amount, cardToken, @event.Name);
        if (!paymentResult.Success)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        ticketPurchase.PurchasedUtc = DateTime.UtcNow;
        ticketPurchase.TotalPaid += amount;

        _unitOfWork.EventTicketPurchaseRepository.Update(ticketPurchase);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> PurchaseTicket(Guid currentMemberId, Guid eventId, string cardToken)
    {
        var (member, @event, ticketPurchase) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPurchaseRepository.GetByMemberId(currentMemberId, eventId));

        if (@event.TicketSettings == null)
        {
            return ServiceResult.Failure("This event is not ticketed");
        }

        if (ticketPurchase?.PurchasedUtc != null)
        {
            return ServiceResult.Failure("You have already purchased a ticket for this event");
        }

        var (paymentSettings, ownerSubscription, membershipSettings, privacySettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, @event.ChapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) != true)
        {
            return ServiceResult.Failure("Payment not made: this group can no longer receive payments");
        }

        var validationResult = MemberCanAttendEvent(@event, member, memberSubscription, membershipSettings, privacySettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        var paymentResult = await _paymentService.MakePayment(paymentSettings, 
            paymentSettings.Currency, member, @event.TicketSettings.Cost, cardToken, @event.Name);
        if (!paymentResult.Success)
        {
            return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
        }

        _unitOfWork.EventResponseRepository.Add(new EventResponse
        {
            EventId = eventId,
            MemberId = member.Id,
            Type = EventResponseType.Yes
        });

        _unitOfWork.EventTicketPurchaseRepository.Add(new EventTicketPurchase
        {
            EventId = eventId,
            MemberId = member.Id,
            PurchasedUtc = DateTime.UtcNow,
            TotalPaid = @event.TicketSettings.Cost
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateMemberResponse(Guid memberId, Guid eventId,
        EventResponseType responseType)
    {
        responseType = NormalizeResponseType(responseType);

        var (member, @event, response) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId));
        
        if (@event.TicketSettings != null)
        {
            return ServiceResult.Failure("Ticketed events cannot be responded to");
        }

        var (membershipSettings, privacySettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, @event.ChapterId));

        var validationResult = MemberCanAttendEvent(@event, member, memberSubscription, membershipSettings, privacySettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (@event.RsvpDeadlinePassed)
        {
            if (response?.Type != EventResponseType.Yes)
            {
                return ServiceResult.Failure("The RSVP deadline has passed");
            }
        }

        if (response == null)
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
            if (response.Type == responseType)
            {
                _unitOfWork.EventResponseRepository.Delete(response);
            }
            else
            {
                response.Type = responseType;
                _unitOfWork.EventResponseRepository.Update(response);
            }            
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static EventResponseType NormalizeResponseType(EventResponseType responseType)
    {
        if (!Enum.IsDefined(typeof(EventResponseType), responseType) || responseType <= EventResponseType.None)
        {
            responseType = EventResponseType.No;
        }

        return responseType;
    }

    private ServiceResult MemberCanAttendEvent(
        Event @event, 
        Member? member, 
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
