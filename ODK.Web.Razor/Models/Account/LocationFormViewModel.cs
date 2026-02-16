using System.ComponentModel;
using ODK.Core.Countries;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Account;

public class LocationFormViewModel : LocationPickerViewModel
{
    [DisplayName("Show distances in")]
    public DistanceUnitType? DistanceUnit { get; set; }

    public IReadOnlyCollection<DistanceUnit> DistanceUnitOptions { get; set; } = [];
}