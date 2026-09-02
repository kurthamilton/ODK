namespace ODK.Web.Razor.Pages.SiteAdmin;

public class PaymentRefundCreateModel : SiteAdminPageModel
{
    public PaymentRefundCreateModel()
    {
    }

    /// <summary>
    /// The payment the form was opened against, where it was reached from one. Absent when the form is
    /// opened cold, which leaves every field blank.
    /// </summary>
    public Guid? PaymentId { get; private set; }

    public void OnGet(Guid? paymentId)
    {
        PaymentId = paymentId;
    }
}
