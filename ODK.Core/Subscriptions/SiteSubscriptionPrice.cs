using ODK.Core.Countries;

namespace ODK.Core.Subscriptions;

public class SiteSubscriptionPrice : IDatabaseEntity
{    
    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public Guid Id { get; set; }

    public double MonthlyAmount { get; set; }

    public Guid SiteSubscriptionId { get; set; }

    public double YearlyAmount { get; set; }
}
