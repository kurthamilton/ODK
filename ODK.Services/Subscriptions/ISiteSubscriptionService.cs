using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionService
{
    Task<ServiceResult> ConfirmMemberSiteSubscription(
        MemberServiceRequest request, Guid siteSubscriptionId, string externalId);

    Task<MemberSiteSubscription?> GetMemberSiteSubscription(
        MemberServiceRequest request);

    Task<PaymentStatusType> GetMemberSiteSubscriptionPaymentCheckoutSessionStatus(
        MemberServiceRequest request, string externalSessionId);

    Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        ServiceRequest request, Guid? memberId);    

    Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberServiceRequest request, Guid priceId, string returnPath);

    Task SyncExpiredSubscriptions(ServiceRequest request);

    Task<ServiceResult> UpdateMemberSiteSubscription(
        MemberServiceRequest request, 
        Guid siteSubscriptionId, 
        SiteSubscriptionFrequency frequency);
}
