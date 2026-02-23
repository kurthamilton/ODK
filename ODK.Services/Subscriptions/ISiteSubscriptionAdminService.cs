using ODK.Core.Subscriptions;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public interface ISiteSubscriptionAdminService
{
    Task<ServiceResult<Guid>> AddSiteSubscription(IMemberServiceRequest request, SiteSubscriptionCreateModel model);

    Task<ServiceResult> AddSiteSubscriptionPrice(
        IMemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model);

    Task DeleteSiteSubscriptionPrice(IMemberServiceRequest request, Guid siteSubscriptionId, Guid siteSubscriptionPriceId);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(IMemberServiceRequest request);

    Task<IReadOnlyCollection<SiteSubscriptionSiteAdminListItemViewModel>> GetSiteSubscriptionSiteAdminListItems(
        IMemberServiceRequest request);

    Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(IMemberServiceRequest request, Guid siteSubscriptionId);

    Task MakeDefault(IMemberServiceRequest request, Guid siteSubscriptionId);

    Task<ServiceResult> UpdateSiteSubscription(
        IMemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionCreateModel model);

    Task<ServiceResult> UpdateSiteSubscriptionEnabled(
        IMemberServiceRequest request, Guid siteSubscriptionId, bool enabled);
}