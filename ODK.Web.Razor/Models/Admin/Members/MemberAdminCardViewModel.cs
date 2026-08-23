using ODK.Core.Chapters;
using ODK.Data.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberAdminCardViewModel
{
    public required Chapter Chapter { get; init; }

    public required MemberChapterWithAvatarDto Member { get; init; }
}
