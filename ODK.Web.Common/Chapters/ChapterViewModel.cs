using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Common.Chapters;
public class ChapterViewModel
{
    public required Chapter Chapter { get; set; }

    public required Member? Member { get; set; }
}
