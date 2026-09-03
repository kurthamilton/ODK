namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// Every payment taken for the platform, whichever group it was for and whether it was for a group at all.
/// </summary>
public class SitePaymentsViewModel
{
    public required IReadOnlyCollection<PaymentItemViewModel> Payments { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
