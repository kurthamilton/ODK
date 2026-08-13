using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

/// <summary>
/// The site's version of one email - what every group's send falls back to, so both fields are required.
/// A group's override posts <see cref="Admin.Chapters.ChapterEmailFormSubmitViewModel"/> instead, where
/// blank means the field is not overridden.
/// </summary>
public class SiteEmailFormSubmitViewModel
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;
}
