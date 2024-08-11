using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class LocationFormViewModel
{
    public double? Lat { get; set; }

    public double? Long { get; set; }

    [DisplayName("Location")]
    [Required]
    public string Name { get; set; } = "";

    public bool InlineButton { get; set; }
}
