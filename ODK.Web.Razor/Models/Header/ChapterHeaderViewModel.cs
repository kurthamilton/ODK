using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header;

public class ChapterHeaderViewModel : OdkComponentViewModel
{
    public ChapterHeaderViewModel(OdkComponentContext context)
        : base(context)
    {
    }

    public required Chapter Chapter { get; init; }

    public required Member? Member { get; init; }

    public required IReadOnlyCollection<ChapterPage> Pages { get; init; }
}