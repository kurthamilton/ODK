using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueFormViewModel : LocationPickerViewModel
{
    public string? Address { get; set; }

    [Required]
    public string? Name { get; set; }
}
