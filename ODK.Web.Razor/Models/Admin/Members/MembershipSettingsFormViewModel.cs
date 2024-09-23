using System.ComponentModel;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MembershipSettingsFormViewModel
{
    [DisplayName("Approve new members")]
    public bool ApproveNewMembers { get; set; }

    [DisplayName("Membership enabled")]
    public bool Enabled { get; set; }

    [DisplayName("Membership disabled after (days)")]
    public int MembershipDisabledAfter { get; set; }

    [DisplayName("Membership expiry warning message (days) before expiry")]
    public int MembershipExpiringWarningDays { get; set; }

    [DisplayName("Trial period (months)")]
    public int TrialPeriodMonths { get; set; }
}
