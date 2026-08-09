using System.ComponentModel;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MembershipSettingsFormViewModel
{
    [DisplayName("Approve new members")]
    public bool ApproveNewMembers { get; set; }

    /// <summary>
    /// Whether the owner's subscription includes member approval, used only to decide whether the switch
    /// renders or an upgrade prompt does. Deliberately not required: this type is bound from the posted
    /// form, which doesn't send it, and it carries no authority - UpdateChapterMembershipSettings re-reads
    /// the owner's features from the database before applying anything.
    /// </summary>
    public bool CanApproveMembers { get; set; }

    [DisplayName("Membership enabled")]
    public bool Enabled { get; set; }

    [DisplayName("Membership disabled after (days)")]
    public int MembershipDisabledAfter { get; set; }

    [DisplayName("Membership expiry warning message (days) before expiry")]
    public int MembershipExpiringWarningDays { get; set; }

    [DisplayName("Trial period (months)")]
    public int TrialPeriodMonths { get; set; }
}
