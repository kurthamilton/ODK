using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Members.ViewModels;

public class ChapterSubscriptionCheckoutStartedViewModel
{
    /// <summary>
    /// The provider key the checkout page hands the provider's own browser script.
    /// </summary>
    public required string ApiPublicKey { get; init; }

    public required Chapter Chapter { get; init; }

    public required ChapterSubscription ChapterSubscription { get; init; }

    public required string ClientSecret { get; init; }

    public required PaymentProviderType PaymentProvider { get; init; }
}