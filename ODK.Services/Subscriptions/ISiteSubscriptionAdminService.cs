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

    /// <summary>
    /// Deletes a subscription nothing depends on. Fails rather than cascading: a subscription any member has
    /// ever been on is part of that member's payment history, and prices have a plan at the payment provider
    /// that has to be deactivated with them, which deleting the prices does.
    /// </summary>
    Task<ServiceResult> DeleteSiteSubscription(IMemberServiceRequest request, Guid siteSubscriptionId);

    /// <summary>
    /// Deletes a price no member holds, deactivating its plan at the payment provider. Fails rather than
    /// cascading: a record naming the price is what a member paid, so the price has to outlive it.
    /// </summary>
    Task<ServiceResult> DeleteSiteSubscriptionPrice(
        IMemberServiceRequest request, Guid siteSubscriptionId, Guid siteSubscriptionPriceId);

    Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(IMemberServiceRequest request);

    Task<SiteAdminMembersViewModel> GetSiteAdminMembersViewModel(IMemberServiceRequest request);

    Task<IReadOnlyCollection<SiteSubscriptionSiteAdminListItemViewModel>> GetSiteSubscriptionSiteAdminListItems(
        IMemberServiceRequest request);

    Task<SiteSubscriptionCreateViewModel> GetSubscriptionCreateViewModel(IMemberServiceRequest request);

    Task<SiteSubscriptionEditViewModel> GetSubscriptionEditViewModel(IMemberServiceRequest request, Guid siteSubscriptionId);

    Task<ServiceResult> MakeDefault(IMemberServiceRequest request, Guid siteSubscriptionId);

    Task<ServiceResult> UpdateSiteSubscription(
        IMemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionCreateModel model);

    Task<ServiceResult> UpdateSiteSubscriptionEnabled(
        IMemberServiceRequest request, Guid siteSubscriptionId, bool enabled);
}