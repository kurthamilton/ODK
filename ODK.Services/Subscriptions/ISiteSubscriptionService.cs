using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<ServiceResult> CancelMemberSiteSubscription(
        MemberServiceRequest request, Guid siteSubscriptionId);

    Task<ServiceResult> ConfirmMemberSiteSubscription(
        MemberServiceRequest request, Guid siteSubscriptionId, string externalId);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        ServiceRequest request, Guid? memberId, Guid? chapterId);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberServiceRequest request, Guid priceId, string returnPath, Guid? chapterId);

    Task SyncExpiredSubscriptions(ServiceRequest request);
}