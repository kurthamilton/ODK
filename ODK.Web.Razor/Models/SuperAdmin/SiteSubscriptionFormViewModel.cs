using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteSubscriptionFormViewModel
{
    [Required]
    public string Description { get; set; } = "";

    public bool Enabled { get; set; }    

    [DisplayName("Fallback")]
    public Guid? FallbackSiteSubscriptionId { get; set; }

    [DisplayName("Group limit")]
    public int? GroupLimit { get; set; }

    [DisplayName("Member limit")]
    public int? MemberLimit { get; set; }

    [DisplayName("Member subscriptions")]
    public bool MemberSubscriptions { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public bool Premium { get; set; }

    [DisplayName("Send member emails")]
    public bool SendMemberEmails { get; set; }

    [Required]
    [DisplayName("Site payment settings")]
    public Guid? SitePaymentSettingId { get; set; }

    public IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; set; } = [];

    public Guid? SiteSubscriptionId { get; set; }
}
