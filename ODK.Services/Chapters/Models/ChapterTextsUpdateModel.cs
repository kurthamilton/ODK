namespace ODK.Services.Chapters.Models;

public class ChapterTextsUpdateModel
{
    public required string? DescriptionHtml { get; init; }

    /// <summary>
    /// Non-nullable because the group must have one, which the update enforces - a caller with nothing to
    /// pass sends an empty string and is told the field is required, rather than sending null and having the
    /// distinction quietly matter.
    /// </summary>
    public required string RegisterTextHtml { get; init; }

    public required string? ShortDescription { get; init; }

    /// <inheritdoc cref="RegisterTextHtml" />
    public required string WelcomeTextHtml { get; init; }
}
