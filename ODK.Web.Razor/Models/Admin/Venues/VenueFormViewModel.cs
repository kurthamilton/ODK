using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueFormViewModel
{
    public Guid ChapterId { get; set; }
    
    public string? Address { get; set; }

    [Required]
    public string? Name { get; set; }

    [DisplayName("Map search")]
    public string? MapQuery { get; set; }
}
