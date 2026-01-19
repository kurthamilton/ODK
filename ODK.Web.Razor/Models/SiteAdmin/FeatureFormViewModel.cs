using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class FeatureFormViewModel
{
    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}