namespace ODK.Services.Payments.Models;

public class PaymentMetadataModel
{
    public Guid? ChapterId { get; set; }

    public Guid? ChapterSubscriptionId { get; set; }

    public Guid? MemberId { get; set; }

    public Guid? PaymentCheckoutSessionId { get; set; }

    public Guid? PaymentId { get; set; }

    public Guid? SiteSubscriptionId { get; set; }

    public static PaymentMetadataModel FromDictionary(IDictionary<string, string> dictionary)
    {
        TryGetGuid(dictionary, "ChapterId", out var chapterId);
        TryGetGuid(dictionary, "ChapterSubscriptionId", out var chapterSubscriptionId);
        TryGetGuid(dictionary, "MemberId", out var memberId);
        TryGetGuid(dictionary, "PaymentCheckoutSessionId", out var paymentCheckoutSessionId);
        TryGetGuid(dictionary, "PaymentId", out var paymentId);
        TryGetGuid(dictionary, "SiteSubscriptionId", out var siteSubscriptionId);

        return new PaymentMetadataModel
        {
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            MemberId = memberId,
            PaymentCheckoutSessionId = paymentCheckoutSessionId,
            PaymentId = paymentId,
            SiteSubscriptionId = siteSubscriptionId
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

        if (SiteSubscriptionId != null)
        {
            dictionary.Add("SiteSubscriptionId", SiteSubscriptionId.Value.ToString());
        }

        return dictionary;
    }

    private static bool TryGetGuid(IDictionary<string, string> dictionary, string key, out Guid? value)
    {
        value = null;

        if (!dictionary.TryGetValue(key, out var stringValue))
        {
            return false; 
        }

        if (!Guid.TryParse(stringValue, out var parsedValue))
        {
            return false;
        }

        value = parsedValue;
        return true;
    }
}
