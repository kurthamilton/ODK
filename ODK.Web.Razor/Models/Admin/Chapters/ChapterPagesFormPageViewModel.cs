using ODK.Core.Pages;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterPagesFormPageViewModel
{
    public bool Hidden { get; set; }

    public string? Title { get; set; } = string.Empty;

    public PageType Type { get; set; }
}