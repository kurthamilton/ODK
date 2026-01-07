using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberImageViewModel : MemberAvatarViewModel
{
    public bool? HasImage { get; init; }

    public MemberImage? Image { get; init; }
}
