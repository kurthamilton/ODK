using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Emails;

public class ChapterEmailsViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterEmail> Emails { get; init; }
}
