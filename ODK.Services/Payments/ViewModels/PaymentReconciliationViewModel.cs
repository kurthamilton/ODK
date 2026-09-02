namespace ODK.Services.Payments.ViewModels;

public class PaymentReconciliationViewModel
{
    /// <summary>
    /// Payments a site admin has told reconciliation to ignore. Listed rather than hidden, because an
    /// instruction nobody can see is one nobody can undo.
    /// </summary>
    public required IReadOnlyCollection<PaymentReconciliationItemViewModel> Ignored { get; init; }

    public required IReadOnlyCollection<PaymentReconciliationItemViewModel> Payments { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
