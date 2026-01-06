using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Components;

public class LocationPickerViewModel
{
    public double? Lat { get; set; }

    public double? Long { get; set; }

    [DisplayName("Location")]
    [Required]
    public string LocationName { get; set; } = string.Empty;
}
