using ODK.Core.Events;
using ODK.Core.Payments;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Events.ViewModels;

public class EventCheckoutPageViewModel : GroupPageViewModel
{
    public required string ClientSecret { get; init; }

    public required string CurrencyCode { get; init; }

    public required Event Event { get; init; }

    public required IPaymentSettings PaymentSettings { get; init; }
}
