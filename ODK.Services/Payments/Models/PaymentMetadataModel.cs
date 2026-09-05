using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;

namespace ODK.Services.Payments.Models;

public class PaymentMetadataModel
{
    public PaymentMetadataModel(
        PlatformType platform,
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
        Platform = platform;
        Reason = reason;
    }

    public PaymentMetadataModel(
        PlatformType platform,
        PaymentReasonType reason,
        Member member,
        SiteSubscriptionPrice siteSubscriptionPrice,
        Guid paymentCheckoutSessionId,
        Guid paymentId)
    {
        MemberId = member.Id;
        PaymentCheckoutSessionId = paymentCheckoutSessionId;
        PaymentId = paymentId;
        Platform = platform;
        Reason = reason;
        SiteSubscriptionPriceId = siteSubscriptionPrice.Id;
    }

    public PaymentMetadataModel(
        PlatformType platform,
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
        Platform = platform;
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

    public PlatformType? Platform { get; private set; }

    /// <summary>
    /// The platform the payment belongs to, for a reader that needs one either way. Metadata held by the
    /// payment provider from before the platform was recorded carries none, and those payments are all
    /// Drunken Knitwits' - so an absent platform means Drunken Knitwits, never
    /// <see cref="PlatformType.Default"/>.
    /// </summary>
    public PlatformType PlatformOrDrunkenKnitwits => Platform ?? PlatformType.DrunkenKnitwits;

    public PaymentReasonType? Reason { get; private set; }

    public Guid? SiteSubscriptionPriceId { get; private set; }

    /// <summary>
    /// The metadata a recurring group subscription has to carry for every renewal of it to be recorded:
    /// what the payment is for, and who and what it is for. Held by the subscription itself, so it names
    /// only what is true of every invoice the subscription will ever issue.
    /// </summary>
    public static PaymentMetadataModel ForChapterSubscription(
        PlatformType platform,
        Guid memberId,
        Guid chapterId,
        Guid chapterSubscriptionId)
        => new()
        {
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            MemberId = memberId,
            Platform = platform,
            Reason = PaymentReasonType.ChapterSubscription
        };

    /// <inheritdoc cref="ForChapterSubscription"/>
    public static PaymentMetadataModel ForSiteSubscription(
        PlatformType platform,
        Guid memberId,
        Guid siteSubscriptionPriceId)
        => new()
        {
            MemberId = memberId,
            Platform = platform,
            Reason = PaymentReasonType.SiteSubscription,
            SiteSubscriptionPriceId = siteSubscriptionPriceId
        };

    public static PaymentMetadataModel FromDictionary(IReadOnlyDictionary<string, string> dictionary)
    {
        dictionary = dictionary.WithComparer(StringComparer.OrdinalIgnoreCase);

        dictionary.TryGetGuidValue(Keys.ChapterId, out var chapterId);
        dictionary.TryGetGuidValue(Keys.ChapterSubscriptionId, out var chapterSubscriptionId);
        dictionary.TryGetGuidValue(Keys.EventTicketPaymentId, out var eventTicketPaymentId);
        dictionary.TryGetGuidValue(Keys.EventId, out var eventId);
        dictionary.TryGetGuidValue(Keys.MemberId, out var memberId);
        dictionary.TryGetGuidValue(Keys.PaymentCheckoutSessionId, out var paymentCheckoutSessionId);
        dictionary.TryGetGuidValue(Keys.PaymentId, out var paymentId);
        dictionary.TryGetEnumValue<PlatformType>(Keys.Platform, out var platform);
        dictionary.TryGetEnumValue<PaymentReasonType>(Keys.Reason, out var reason);
        dictionary.TryGetGuidValue(Keys.SiteSubscriptionPriceId, out var siteSubscriptionPriceId);

        return new PaymentMetadataModel
        {
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            EventId = eventId,
            EventTicketPaymentId = eventTicketPaymentId,
            MemberId = memberId,
            PaymentCheckoutSessionId = paymentCheckoutSessionId,
            PaymentId = paymentId,
            Platform = platform,
            Reason = reason,
            SiteSubscriptionPriceId = siteSubscriptionPriceId
        };
    }

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var dictionary = new Dictionary<string, string>();

        if (ChapterId != null)
        {
            dictionary.Add(Keys.ChapterId, ChapterId.Value.ToString());
        }

        if (ChapterSubscriptionId != null)
        {
            dictionary.Add(Keys.ChapterSubscriptionId, ChapterSubscriptionId.Value.ToString());
        }

        if (EventId != null)
        {
            dictionary.Add(Keys.EventId, EventId.Value.ToString());
        }

        if (EventTicketPaymentId != null)
        {
            dictionary.Add(Keys.EventTicketPaymentId, EventTicketPaymentId.Value.ToString());
        }

        if (MemberId != null)
        {
            dictionary.Add(Keys.MemberId, MemberId.Value.ToString());
        }

        if (PaymentCheckoutSessionId != null)
        {
            dictionary.Add(Keys.PaymentCheckoutSessionId, PaymentCheckoutSessionId.Value.ToString());
        }

        if (PaymentId != null)
        {
            dictionary.Add(Keys.PaymentId, PaymentId.Value.ToString());
        }

        if (Platform != null)
        {
            dictionary.Add(Keys.Platform, Platform.Value.ToString());
        }

        if (Reason != null)
        {
            dictionary.Add(Keys.Reason, Reason.Value.ToString());
        }

        if (SiteSubscriptionPriceId != null)
        {
            dictionary.Add(Keys.SiteSubscriptionPriceId, SiteSubscriptionPriceId.Value.ToString());
        }

        return dictionary;
    }

    /// <summary>
    /// The names the keys are written under, and read back by. One statement of each, because a payment
    /// provider holds metadata written by a version of this app that has long since been replaced, and a
    /// key renamed on one side alone stops matching without failing.
    /// </summary>
    public static class Keys
    {
        public const string ChapterId = "ChapterId";

        public const string ChapterSubscriptionId = "ChapterSubscriptionId";

        public const string EventId = "EventId";

        public const string EventTicketPaymentId = "EventTicketPaymentId";

        public const string MemberId = "MemberId";

        public const string PaymentCheckoutSessionId = "PaymentCheckoutSessionId";

        public const string PaymentId = "PaymentId";

        public const string Platform = "Platform";

        public const string Reason = "Reason";

        public const string SiteSubscriptionPriceId = "SiteSubscriptionPriceId";
    }
}
