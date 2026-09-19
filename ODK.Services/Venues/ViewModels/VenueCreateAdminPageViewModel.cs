using ODK.Core.Chapters;

namespace ODK.Services.Venues.ViewModels;

/// <summary>
/// What the venue create form needs beyond an empty form. A new venue has no location of its own for
/// the picker to search around, so the group's stands in for one.
/// </summary>
public class VenueCreateAdminPageViewModel
{
    /// <summary>
    /// Where the group is, for the venue picker to prefer suggestions near. Null where the group has
    /// not said where it is.
    /// </summary>
    public required ChapterLocation? ChapterLocation { get; init; }
}
