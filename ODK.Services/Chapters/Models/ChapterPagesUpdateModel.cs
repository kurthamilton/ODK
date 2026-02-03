namespace ODK.Services.Chapters.Models;

public class ChapterPagesUpdateModel
{
    public required IReadOnlyCollection<ChapterPageUpdateModel> Pages { get; init; }
}