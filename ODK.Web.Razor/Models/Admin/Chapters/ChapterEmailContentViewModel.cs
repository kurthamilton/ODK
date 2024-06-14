using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailContentViewModel
{
    public ChapterEmailContentViewModel(Chapter chapter, ChapterEmail email)
    {
        Chapter = chapter;
        Email = email;
    }

    public Chapter Chapter { get; }

    public ChapterEmail Email { get; }
}
