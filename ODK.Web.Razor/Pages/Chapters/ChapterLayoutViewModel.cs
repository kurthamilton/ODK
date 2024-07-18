using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters;

public class ChapterLayoutViewModel
{
    public ChapterLayoutViewModel(ChapterViewModel viewModel)
    {
        Chapter = viewModel.Chapter;
        Member = viewModel.Member;
    }

    public Chapter Chapter { get; set; }

    public required IHtmlContent Content { get; set; }

    public Member? Member { get; set; }
}
