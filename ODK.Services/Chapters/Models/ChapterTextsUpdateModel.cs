namespace ODK.Services.Chapters.Models;

public class ChapterTextsUpdateModel
{
    public required string? DescriptionHtml { get; init; }

    public required string? RegisterTextHtml { get; init; }

    public required string? ShortDescription { get; init; }

    public required string? WelcomeTextHtml { get; init; }
}