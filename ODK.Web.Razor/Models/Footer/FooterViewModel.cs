using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Footer;

public class FooterViewModel
{
    public Chapter? Chapter { get; init; }

    public ChapterLinks? Links { get; init; }
}