using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormSubmitViewModel
{
    [Required]
    [NonNegative]
    public decimal? Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [DisplayName("Duration (months)")]
    [Required]
    public int? DurationMonths { get; set; }

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool Recurring { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
}