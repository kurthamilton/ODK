using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Footer;

public class FooterViewModel : OdkComponentViewModel
{
    public FooterViewModel(OdkComponentContext context)
        : base(context)
    {
    }

    public Chapter? Chapter { get; init; }

    public ChapterLinks? Links { get; init; }
}