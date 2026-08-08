using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class ReferralCampaignFormViewModel
{
    /// <summary>
    /// A plain local date from an &lt;input type="date"&gt;, or null for a campaign that never expires.
    /// The service resolves it to a UTC instant.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "Expires")]
    public DateTime? ExpiresLocalDate { get; set; }

    /// <summary>Shown to the member on the refer page. HTML from the rich text editor.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Subject line of the referral email. Plain text.</summary>
    [Display(Name = "Email subject")]
    public string EmailSubject { get; set; } = string.Empty;

    /// <summary>
    /// Body of the referral email. HTML from the rich text editor, interpolated with the member.fullName,
    /// url and referral.id tokens when sent.
    /// </summary>
    [Display(Name = "Email text")]
    public string EmailText { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}
