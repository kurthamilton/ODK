using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class FeatureFormViewModel
{
    [Required]
    public string Description { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";
}
