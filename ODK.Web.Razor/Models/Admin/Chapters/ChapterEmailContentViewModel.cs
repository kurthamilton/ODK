using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailContentViewModel
{
    public ChapterEmailContentViewModel(Chapter chapter, ChapterEmail email, bool canEdit)
    {
        CanEdit = canEdit;
        Chapter = chapter;
        Email = email;
    }

    public bool CanEdit { get; }

    public Chapter Chapter { get; }

    public ChapterEmail Email { get; }
}
