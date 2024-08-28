using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueFormViewModel
{
    public string? Address { get; set; }

    public double? Lat { get; set; }

    [Required]
    [DisplayName("Location")]
    public string? LocationName { get; set; }

    public double? Long { get; set; }

    [Required]
    public string? Name { get; set; }
}
