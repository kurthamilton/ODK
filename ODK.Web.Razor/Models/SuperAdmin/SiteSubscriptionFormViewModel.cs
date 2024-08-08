using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteSubscriptionFormViewModel
{
    [Required]
    public string Description { get; set; } = "";

    public bool Enabled { get; set; }

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
}
