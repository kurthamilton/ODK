namespace ODK.Services.Chapters.Models;

public class UpdateChapterLinks
{
    public required string? Facebook { get; init; }

    public required string? Instagram { get; init; }

    public required bool InstagramFeed { get; init; }

    public required string? Twitter { get; init; }

    public required string? WhatsApp { get; init; }
}