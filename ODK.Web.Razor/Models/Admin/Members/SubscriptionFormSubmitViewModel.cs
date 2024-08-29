using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormSubmitViewModel
{
    [Required]
    public double? Amount { get; set; }    

    [Required]
    public string Description { get; set; } = "";

    [DisplayName("Duration (months)")]
    [Required]
    public int? DurationMonths { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Title { get; set; } = "";

    [Required]
    public SubscriptionType Type { get; set; }
}
