using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class LocationFormViewModel
{
    public double? Lat { get; set; }

    public double? Long { get; set; }

    [DisplayName("Location")]
    [Required]
    public string LocationName { get; set; } = "";
}
