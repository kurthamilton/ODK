namespace ODK.Web.Razor.Models.Admin.Emails;

public class ChapterEmailSettingsFormViewModel : ChapterEmailSettingsFormSubmitViewModel
{
    /// <summary>
    /// Set when the group's subscription does not include custom emails. Anything already set is still
    /// shown - it keeps being used - but nothing on the form can be changed.
    /// </summary>
    public required bool ReadOnly { get; init; }

    /// <summary>
    /// The site's titles, shown beside each box so a group can see what leaving it empty gives it.
    /// Render-only, so they sit here rather than on the submit model.
    /// </summary>
    public required string SiteAdminTitle { get; init; }

    /// <inheritdoc cref="SiteAdminTitle" />
    public required string SiteMemberTitle { get; init; }
}
