using System.ComponentModel;

namespace ODK.Web.Razor.Models.Admin.Emails;

public class ChapterEmailSettingsFormSubmitViewModel
{
    /// <summary>
    /// Optional, unlike the site's: blank means the group inherits the site's title rather than that it
    /// has no title. Nothing here is required, so there is no validation attribute to match.
    /// </summary>
    [DisplayName("Admin Title")]
    public string? AdminTitle { get; set; }

    /// <inheritdoc cref="AdminTitle" />
    [DisplayName("Member Title")]
    public string? MemberTitle { get; set; }
}
