using ODK.Core.Chapters;

namespace ODK.Services.Emails.ViewModels;

public class ChapterEmailsAdminPageViewModel
{
    /// <summary>
    /// Whether the group's subscription includes custom emails. Anything it has already set keeps being
    /// used either way; this only decides whether it can be changed.
    /// </summary>
    public required bool CanEdit { get; init; }

    public required IReadOnlyCollection<ChapterEmailListItemViewModel> Emails { get; init; }

    /// <summary>
    /// The site's titles, shown beside the group's own boxes so it can see what leaving one empty gives it.
    /// Only these two, rather than the whole of the site's email settings, which are not a group's business.
    /// </summary>
    public required string SiteAdminTitle { get; init; }

    /// <inheritdoc cref="SiteAdminTitle" />
    public required string SiteMemberTitle { get; init; }

    /// <summary>
    /// Null until the group saves the form for the first time, which is the same thing as inheriting every
    /// value from the site.
    /// </summary>
    public required ChapterEmailSettings? Settings { get; init; }
}
