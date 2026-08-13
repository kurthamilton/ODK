namespace ODK.Web.Razor.Models.Admin.Chapters;

/// <summary>
/// A group's override of one email. Neither field is required: blank means the group is not overriding it,
/// so the send uses the site's. The site's own email posts
/// <see cref="SiteAdmin.SiteEmailFormSubmitViewModel"/>, where both are required.
/// </summary>
public class ChapterEmailFormSubmitViewModel
{
    public string? Content { get; set; }

    public string? Subject { get; set; }
}
