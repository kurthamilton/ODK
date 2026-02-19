using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Chapters;

public class ManageChapterLinkViewModel
{
    public required Chapter Chapter { get; init; }

    public string? Class { get; init; }

    public string? Justify { get; init; }
}