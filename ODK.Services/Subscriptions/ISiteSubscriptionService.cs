using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<bool> CompleteSiteSubscriptionCheckoutSession(Guid memberId, Guid siteSubscriptionPriceId, string sessionId);

    Task<ServiceResult> ConfirmMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, string externalId);

    Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(Guid? memberId);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberServiceRequest request, Guid priceId, string returnPath);

    Task SyncExpiredSubscriptions(IHttpRequestContext httpRequestContext);

    Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId, 
        SiteSubscriptionFrequency frequency);
}
