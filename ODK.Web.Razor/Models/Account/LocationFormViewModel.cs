using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Account;

public class LocationFormViewModel
{
    [DisplayName("Show distances in")]
    public Guid? DistanceUnit { get; set; }

    public IReadOnlyCollection<DistanceUnit> DistanceUnitOptions { get; set; } = [];

    public double? Lat { get; set; }

    public double? Long { get; set; }

    [DisplayName("Location")]
    [Required]
    public string Name { get; set; } = "";

    public bool InlineButton { get; set; }

    [DisplayName("Time zone")]
    [Required]
    public string TimeZoneId { get; set; } = "";
}
