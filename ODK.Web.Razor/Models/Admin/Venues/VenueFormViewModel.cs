using System.ComponentModel;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueFormViewModel : LocationPickerViewModel
{
    [DisplayName("Additional information")]
    public string? AdditionalInfo { get; set; }

    /* Not required: left empty the venue takes the name the lookup returned, which is the usual case. */
    public string? Name { get; set; }
}
