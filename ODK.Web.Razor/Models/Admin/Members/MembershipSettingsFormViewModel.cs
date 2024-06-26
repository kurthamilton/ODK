﻿using System.ComponentModel;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MembershipSettingsFormViewModel
{
    [DisplayName("Membership enabled")]
    public bool Enabled { get; set; }

    [DisplayName("Membership disabled after")]
    public int MembershipDisabledAfter { get; set; }

    [DisplayName("Trial period (months)")]
    public int TrialPeriodMonths { get; set; }
}
