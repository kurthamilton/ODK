using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentConfirmViewModel
{
    /// <summary>The group the checkout was made under, where it was made under one.</summary>
    public required Chapter? Chapter { get; init; }

    public required PaymentConfirmScope Scope { get; init; }

    /// <summary>The payment provider's own id for the session - PaymentCheckoutSession.SessionId.</summary>
    public required string SessionId { get; init; }
}
