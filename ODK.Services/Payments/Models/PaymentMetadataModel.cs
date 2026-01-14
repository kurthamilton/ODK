using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Subscriptions;

namespace ODK.Services.Payments.Models;

public class PaymentMetadataModel
{
    public PaymentMetadataModel(
        PaymentReasonType reason,
        Member member,
        ChapterSubscription chapterSubscription,
        Guid paymentCheckoutSessionId,
        Guid paymentId)
    {
        ChapterId = chapterSubscription.ChapterId;
        ChapterSubscriptionId = chapterSubscription.Id;
        MemberId = member.Id;
        PaymentCheckoutSessionId = paymentCheckoutSessionId;
        PaymentId = paymentId;
        Reason = reason;
    }

    public PaymentMetadataModel(
        PaymentReasonType reason,
        Member member,
        SiteSubscriptionPrice siteSubscriptionPrice,
        Guid paymentCheckoutSessionId,
        Guid paymentId)
    {
        MemberId = member.Id;
        PaymentCheckoutSessionId = paymentCheckoutSessionId;
        PaymentId = paymentId;
        Reason = reason;
        SiteSubscriptionPriceId = siteSubscriptionPrice.Id;
    }

    public PaymentMetadataModel(
        PaymentReasonType reason,
        Member member,
        EventTicketPayment eventTicketPayment,
        Guid paymentCheckoutSessionId)
    {
        EventId = eventTicketPayment.EventId;
        EventTicketPaymentId = eventTicketPayment.Id;
        MemberId = member.Id;
        PaymentCheckoutSessionId = paymentCheckoutSessionId;
        PaymentId = eventTicketPayment.PaymentId;
        Reason = reason;
    }

    private PaymentMetadataModel()
    {
    }

    public Guid? ChapterId { get; private set; }

    public Guid? ChapterSubscriptionId { get; private set; }

    public Guid? EventId { get; private set; }

    public Guid? EventTicketPaymentId { get; private set; }

    public Guid? MemberId { get; private set; }

    public Guid? PaymentCheckoutSessionId { get; private set; }

    public Guid? PaymentId { get; private set; }

    public PaymentReasonType? Reason { get; private set; }

    public Guid? SiteSubscriptionPriceId { get; private set; }

    public static PaymentMetadataModel FromDictionary(IDictionary<string, string> dictionary)
    {
        dictionary.TryGetGuidValue("ChapterId", out var chapterId);
        dictionary.TryGetGuidValue("ChapterSubscriptionId", out var chapterSubscriptionId);
        dictionary.TryGetGuidValue("EventTicketPaymentId", out var eventTicketPaymentId);
        dictionary.TryGetGuidValue("EventId", out var eventId);
        dictionary.TryGetGuidValue("MemberId", out var memberId);
        dictionary.TryGetGuidValue("PaymentCheckoutSessionId", out var paymentCheckoutSessionId);
        dictionary.TryGetGuidValue("PaymentId", out var paymentId);
        dictionary.TryGetEnumValue<PaymentReasonType>("Reason", out var reason);
        dictionary.TryGetGuidValue("SiteSubscriptionPriceId", out var siteSubscriptionPriceId);

        return new PaymentMetadataModel
        {
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            EventId = eventId,
            EventTicketPaymentId = eventTicketPaymentId,
            MemberId = memberId,
            PaymentCheckoutSessionId = paymentCheckoutSessionId,
            PaymentId = paymentId,
            Reason = reason,
            SiteSubscriptionPriceId = siteSubscriptionPriceId
        };
    }

    public IDictionary<string, string> ToDictionary()
    {
        var dictionary = new Dictionary<string, string>();

        if (ChapterId != null)
        {
            dictionary.Add("ChapterId", ChapterId.Value.ToString());
        }

        if (ChapterSubscriptionId != null)
        {
            dictionary.Add("ChapterSubscriptionId", ChapterSubscriptionId.Value.ToString());
        }

        if (EventId != null)
        {
            dictionary.Add("EventId", EventId.Value.ToString());
        }

        if (EventTicketPaymentId != null)
        {
            dictionary.Add("EventTicketPaymentId", EventTicketPaymentId.Value.ToString());
        }

        if (MemberId != null)
        {
            dictionary.Add("MemberId", MemberId.Value.ToString());
        }

        if (PaymentCheckoutSessionId != null)
        {
            dictionary.Add("PaymentCheckoutSessionId", PaymentCheckoutSessionId.Value.ToString());
        }

        if (PaymentId != null)
        {
            dictionary.Add("PaymentId", PaymentId.Value.ToString());
        }

        if (Reason != null)
        {
            dictionary.Add("Reason", Reason.Value.ToString());
        }

        if (SiteSubscriptionPriceId != null)
        {
            dictionary.Add("SiteSubscriptionPriceId", SiteSubscriptionPriceId.Value.ToString());
        }

        return dictionary;
    }
}