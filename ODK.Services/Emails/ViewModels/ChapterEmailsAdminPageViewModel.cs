using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails.ViewModels;

public class ChapterEmailsAdminPageViewModel
{
    /// <summary>
    /// Whether the group's subscription includes custom emails. Anything it has already set keeps being
    /// used either way; this only decides whether it can be changed.
    /// </summary>
    public required bool CanEdit { get; init; }

    public required IReadOnlyCollection<ChapterEmail> Emails { get; init; }

    /// <summary>
    /// Null until the group saves the form for the first time, which is the same thing as inheriting every
    /// value from the site.
    /// </summary>
    public required ChapterEmailSettings? Settings { get; init; }
}
