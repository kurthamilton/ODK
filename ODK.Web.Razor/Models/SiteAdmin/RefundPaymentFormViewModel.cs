using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class RefundPaymentFormViewModel : RefundPaymentFormSubmitViewModel
{
    /// <summary>
    /// The payment's currency, for stating what the amount is denominated in. The field posts a bare
    /// number: the currency is the payment's and is not the site admin's to change.
    /// </summary>
    public required Currency Currency { get; init; }

    /// <summary>
    /// The payment being refunded. Carried in the form's action rather than posted, so a form cannot name
    /// a payment other than the one it was rendered for.
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// The most the provider will give back: what the payment took, less anything already refunded. Caps
    /// the field as well as filling it in, so the ordinary case is one click.
    /// </summary>
    public required decimal RefundableAmount { get; init; }
}
