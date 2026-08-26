namespace ODK.Web.Razor.Models.SiteAdmin;

public class SitePaymentSettingsFormViewModel : SitePaymentSettingsFormSubmitViewModel
{
    /// <summary>
    /// Whether these are the settings the platform is transacting through. Render-only: an active row
    /// cannot be switched off, so the form shows its enabled state rather than offering it.
    /// </summary>
    public required bool Active { get; init; }

    /// <summary>
    /// The settings being edited, or null when the form creates a new row. Render-only: it says which
    /// fields are still open, since a row's provider is fixed once anything has been transacted under it.
    /// </summary>
    public required Guid? SitePaymentSettingId { get; init; }
}
