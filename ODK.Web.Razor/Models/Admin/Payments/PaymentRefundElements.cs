namespace ODK.Web.Razor.Models.Admin.Payments;

/// <summary>
/// The id a payment's refund dialog is found by. Named here rather than built at each use site because a
/// payments table renders the trigger and the dialog in different places - the trigger in the row, the
/// dialog after the table - and the two have to agree on which payment they are for.
/// </summary>
public static class PaymentRefundElements
{
    public static string ModalId(Guid paymentId) => $"refund-payment-modal-{paymentId}";
}
