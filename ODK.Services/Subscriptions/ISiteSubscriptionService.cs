using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<ServiceResult> CancelMemberSiteSubscription(IMemberServiceRequest request);

    Task<ServiceResult> ConfirmMemberSiteSubscription(
        IMemberServiceRequest request, Guid siteSubscriptionId, string externalId);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        IServiceRequest request, Guid? chapterId);

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberServiceRequest request, Guid priceId, string returnPath, Guid? chapterId);
}