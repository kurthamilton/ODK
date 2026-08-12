using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Emails;

public class ChapterEmailsViewModel
{
    /// <summary>
    /// Whether the group's subscription includes custom emails, which is what decides whether the settings
    /// form can be changed. The templates themselves are listed either way.
    /// </summary>
    public required bool CanEdit { get; init; }

    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterEmail> Emails { get; init; }

    /// <summary>
    /// Null until the group saves the settings form for the first time, which is the same as inheriting
    /// every value from the site.
    /// </summary>
    public required ChapterEmailSettings? Settings { get; init; }
}
