using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionsDto
{
    public required Chapter? Chapter { get; init; }

    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required Currency? Currency { get; init; }

    public required MemberSiteSubscription? CurrentMemberSubscription { get; init; }

    public required IPaymentSettings? PaymentSettings { get; init; }
    
    public required IReadOnlyCollection<SiteSubscriptionDto> Subscriptions { get; init; }
}
