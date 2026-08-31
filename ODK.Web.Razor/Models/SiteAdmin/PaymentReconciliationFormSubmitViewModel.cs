namespace ODK.Web.Razor.Models.SiteAdmin;

/// <summary>
/// The payments a bulk reconciliation action was pressed against.
/// </summary>
/// <remarks>
/// The ids are posted rather than re-queried, so the action covers exactly the rows the site admin was
/// looking at. Re-running the query would act on whatever is pending by the time the form arrives, which is
/// not what they saw and not what they agreed to.
/// </remarks>
public class PaymentReconciliationFormSubmitViewModel
{
    public Guid[] PaymentIds { get; init; } = [];
}
