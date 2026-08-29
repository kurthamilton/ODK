using ODK.Core.Events;
using ODK.Core.Payments;
using ODK.Core.Venues;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Events.ViewModels;

public class EventCheckoutPageViewModel : GroupPageViewModel
{
    /// <summary>
    /// The provider key the checkout page hands the provider's own browser script.
    /// </summary>
    public required string ApiPublicKey { get; init; }

    public required string ClientSecret { get; init; }

    public required PaymentProviderType PaymentProvider { get; init; }

    public required Event Event { get; init; }

    public required Venue? Venue { get; init; }
}