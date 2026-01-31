using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SiteAdminAdminMembersViewModel
{
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    public required Chapter Chapter { get; init; }
}
