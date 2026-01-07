using ODK.Core.Subscriptions;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionAdminService
{
    Task<ServiceResult> AddSiteSubscription(MemberServiceRequest request, SiteSubscriptionCreateModel model);

    Task<ServiceResult> AddSiteSubscriptionPrice(
        MemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model);

    Task DeleteSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(MemberServiceRequest request);

    Task<IReadOnlyCollection<SiteSubscriptionSuperAdminListItemViewModel>> GetSiteSubscriptionSuperAdminListItems(
        MemberServiceRequest request);

    Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(Guid currentMemberId, Guid siteSubscriptionId);

    Task MakeDefault(MemberServiceRequest request, Guid siteSubscriptionId);

    Task<ServiceResult> UpdateSiteSubscription(
        MemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionCreateModel model);

    Task<ServiceResult> UpdateSiteSubscriptionEnabled(Guid currentMemberId, Guid siteSubscriptionId,
        bool enabled);
}
