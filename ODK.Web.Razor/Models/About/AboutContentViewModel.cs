using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.About;

public class AboutContentViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
