using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Components;

public class LocationPickerViewModel
{
    /// <summary>
    /// Identifies the selected suggestion in the service the picker searched, written by its script.
    /// Empty when a location was typed rather than picked, or when it predates the picker recording one.
    /// </summary>
    public string? ExternalId { get; set; }

    public double? Lat { get; set; }

    public double? Long { get; set; }

    /// <summary>
    /// What to search for, for a form that knows it but must not put it in the box: a value there is a
    /// location the picker has resolved, and text it never resolved would post no place at all.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Where to prefer suggestions near, so a search starts local rather than worldwide. Set by a page
    /// that knows somewhere closer than the picker's own value - a group for its venues - and left null
    /// otherwise, which prefers nowhere. A preference and not a filter: a place on the other side of the
    /// world is still found by name.
    /// </summary>
    public LatLong? SearchNear { get; set; }

    [DisplayName("Location")]
    [Required]
    public string LocationName { get; set; } = string.Empty;
}