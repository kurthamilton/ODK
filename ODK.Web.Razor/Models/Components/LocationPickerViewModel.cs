using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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

    [DisplayName("Location")]
    [Required]
    public string LocationName { get; set; } = string.Empty;
}