namespace ODK.Web.Razor.Models.Admin.Members;

public class MembershipSettingsFormViewModel : MembershipSettingsFormSubmitViewModel
{
    /// <summary>
    /// Whether the owner's subscription includes member approval, deciding whether the switch renders or
    /// an upgrade prompt does.
    /// </summary>
    public required bool CanApproveMembers { get; init; }
}
