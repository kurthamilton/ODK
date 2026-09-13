using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMigrationAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Null until the organiser has saved anything, which is the usual state: most groups never moved
    /// here from anywhere.
    /// </summary>
    public required ChapterMigration? Migration { get; init; }

    /// <summary>
    /// The moved page's address including the site, because an organiser's next step is to paste it
    /// somewhere that is not this site.
    /// </summary>
    public required string MovedPageUrl { get; init; }
}
