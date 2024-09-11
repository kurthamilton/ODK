using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterLinksFormViewModel : ChapterLinksFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }    

    public required MemberSiteSubscription? OwnerSubscription { get; init; }
}
