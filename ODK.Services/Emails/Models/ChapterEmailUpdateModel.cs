namespace ODK.Services.Emails.Models;

/// <summary>
/// A group's override of one email. Both fields are optional and independent: blank means the group is not
/// overriding that field, so the send uses the site's. Blanking both removes the override.
/// </summary>
public class ChapterEmailUpdateModel
{
    public required string? HtmlContent { get; init; }

    public required string? Subject { get; init; }
}
