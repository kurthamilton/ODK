namespace ODK.Services.Chapters.Models;

public class ChapterMigrationUpdateModel
{
    public required string? MessageHtml { get; init; }

    /// <summary>
    /// Whether the moved page is public. Turning it on stamps the move date; turning it off clears it and
    /// leaves the wording alone, so an organiser can take the page down without losing what they wrote.
    /// </summary>
    public required bool Moved { get; init; }

    public required string? PreviousPlatformName { get; init; }
}
