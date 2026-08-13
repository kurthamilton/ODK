namespace ODK.Web.Razor.Models.Admin.Chapters;

/// <summary>
/// A group's override of one email. Neither field is required: blank means the group is not overriding it,
/// so the send uses the site's. The site's own email posts
/// <see cref="SiteAdmin.SiteEmailFormSubmitViewModel"/>, where both are required.
/// </summary>
public class ChapterEmailFormSubmitViewModel
{
    public string? Content { get; set; }

    /// <summary>
    /// Whether the group overrides the body, from its own Customise switch. Posted rather than inferred from
    /// <see cref="Content"/> arriving blank: the form locks a field the group may not write, and a locked
    /// field posts nothing, so blank means "nothing to say" rather than "stop overriding this".
    /// </summary>
    public bool OverrideContent { get; set; }

    /// <inheritdoc cref="OverrideContent" />
    public bool OverrideSubject { get; set; }

    public string? Subject { get; set; }
}
