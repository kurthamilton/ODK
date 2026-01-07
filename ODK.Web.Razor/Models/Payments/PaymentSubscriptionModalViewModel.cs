using Microsoft.AspNetCore.Html;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentSubscriptionModalViewModel
{
    public required string Action { get; init; }

    public Func<object?, IHtmlContent>? FormContentFunc { get; init; }

    public required string Id { get; init; }

    public required PaymentSubscriptionFormViewModel PaymentForm { get; init; }

    public required IPaymentSettings? PaymentSettings { get; init; }

    public string? StripeSessionId { get; init; }

    public required string Title { get; init; }
}
