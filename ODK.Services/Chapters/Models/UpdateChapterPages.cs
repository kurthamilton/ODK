namespace ODK.Services.Chapters.Models;

public class UpdateChapterPages
{
    public required IReadOnlyCollection<UpdateChapterPage> Pages { get; init; }
}