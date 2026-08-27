using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IStripeWebhookAdminService
{
    Task<SiteAdminStripeWebhooksViewModel> GetStripeWebhooksViewModel(IMemberServiceRequest request);
}
