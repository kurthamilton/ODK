using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class FeatureFormViewModel
{
    [Required]
    [DisplayName("Description")]
    public string DescriptionHtml { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}