using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Account;

public class LocationFormViewModel : LocationPickerViewModel
{
    [DisplayName("Show distances in")]
    public Guid? DistanceUnit { get; set; }

    public IReadOnlyCollection<DistanceUnit> DistanceUnitOptions { get; set; } = [];    

    [DisplayName("Time zone")]
    [Required]
    public string TimeZoneId { get; set; } = string.Empty;
}
