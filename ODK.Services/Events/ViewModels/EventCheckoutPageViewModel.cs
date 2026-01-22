using ODK.Core.Events;
using ODK.Core.Payments;
using ODK.Core.Venues;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Events.ViewModels;

public class EventCheckoutPageViewModel : GroupPageViewModel
{
    public required string ClientSecret { get; init; }

    public required Event Event { get; init; }

    public required SitePaymentSettings PaymentSettings { get; init; }

    public required Venue? Venue { get; init; }
}