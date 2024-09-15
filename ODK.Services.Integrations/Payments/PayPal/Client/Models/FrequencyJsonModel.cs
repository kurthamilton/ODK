using Newtonsoft.Json;
using ODK.Core.Subscriptions;

namespace ODK.Services.Integrations.Payments.PayPal.Client.Models;

public class FrequencyJsonModel
{
    [JsonProperty("interval_count")]
    public int IntervalCount { get; set; }

    [JsonProperty("interval_unit")]
    public string IntervalUnit { get; set; } = "";

    [JsonIgnore]
    public SiteSubscriptionFrequency OdkFrequency => IntervalUnit switch
    {
        "MONTH" => SiteSubscriptionFrequency.Monthly,
        "YEAR" => SiteSubscriptionFrequency.Yearly,
        _ => throw new NotSupportedException($"Invalid frequency")
    };

    public static FrequencyJsonModel ForFrequency(SiteSubscriptionFrequency frequency)
    {
        return frequency switch
        {
            SiteSubscriptionFrequency.Monthly => new FrequencyJsonModel
            {
                IntervalCount = 1,
                IntervalUnit = "MONTH"
            },
            SiteSubscriptionFrequency.Yearly => new FrequencyJsonModel
            {
                IntervalCount = 1,
                IntervalUnit = "YEAR"
            },
            _ => throw new NotSupportedException($"Invalid frequency")
        };
    }
}
