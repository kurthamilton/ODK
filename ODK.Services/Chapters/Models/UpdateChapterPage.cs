using ODK.Core.Pages;

namespace ODK.Services.Chapters.Models;

public class UpdateChapterPage
{
    public bool Hidden { get; init; }

    public string? Title { get; init; }

    public PageType Type { get; init; }
}