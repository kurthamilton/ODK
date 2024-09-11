using System.ComponentModel;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterLinksFormViewModel
{
    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    public MemberSiteSubscription? OwnerSubscription { get; set; }

    public string? Twitter { get; set; }

    [DisplayName("Show Instagram feed")]
    public bool ShowInstagramFeed { get; set; }
}
