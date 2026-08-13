namespace ODK.Services.Emails.Models;

/// <summary>
/// A group's override of one email. Subject and body are independent: a field the group does not override
/// uses the site's, and overriding neither removes the override altogether.
/// </summary>
/// <remarks>
/// Whether a field is overridden is stated by its own flag rather than inferred from the wording being
/// absent. A form cannot post a field it has locked, so absence means "nothing to say about this field" -
/// which is not the same as "stop overriding it", and reading it as the latter wipes wording the group never
/// touched. With the flag set and no wording supplied, whatever is stored stands.
/// </remarks>
public class ChapterEmailUpdateModel
{
    public required string? HtmlContent { get; init; }

    public required bool OverrideHtmlContent { get; init; }

    public required bool OverrideSubject { get; init; }

    public required string? Subject { get; init; }
}
