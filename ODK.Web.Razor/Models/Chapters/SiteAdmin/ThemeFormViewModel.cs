namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class ThemeFormViewModel : ThemeFormSubmitViewModel
{
    /// <summary>
    /// Renders the current theme without letting it be changed, for an owner whose subscription doesn't
    /// include the feature.
    /// </summary>
    public required bool ReadOnly { get; init; }
}
