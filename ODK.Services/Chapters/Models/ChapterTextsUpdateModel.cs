namespace ODK.Services.Chapters.Models;

public class ChapterTextsUpdateModel
{
    public required string? Description { get; init; }

    public required string? RegisterText { get; init; }

    public required string? ShortDescription { get; init; }

    public required string? WelcomeText { get; init; }
}