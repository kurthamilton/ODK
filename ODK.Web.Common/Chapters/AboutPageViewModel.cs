using System.Collections.Generic;
using ODK.Core.Chapters;

namespace ODK.Web.Common.Chapters;
public class AboutPageViewModel : ChapterViewModel
{
    public required IReadOnlyCollection<ChapterQuestion> Questions { get; set; }
}
