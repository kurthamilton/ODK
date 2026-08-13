using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Emails;

public class EmailBadgesViewModel
{
    /// <summary>
    /// Whether the group overrides anything. False on the site's own copy of a template, which has nothing
    /// to be a customisation of.
    /// </summary>
    public required bool Customised { get; init; }

    public required EmailRecipientType RecipientType { get; init; }
}
