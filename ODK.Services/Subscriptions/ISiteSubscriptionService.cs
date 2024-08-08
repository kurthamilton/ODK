namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto();
}
