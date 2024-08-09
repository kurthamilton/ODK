namespace ODK.Web.Razor.Models.Chapters;

public class ChapterTilesViewModel
{
    public required IReadOnlyCollection<ChapterTileViewModel> Chapters { get; set; }

    public required string Title { get; set; }
}
