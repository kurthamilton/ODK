using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Web.Razor.Models.Home;

public class HomeContentViewModel
{
    public required SiteSubscriptionsViewModel SiteSubscriptions { get; init; }
}
