using Microsoft.AspNetCore.Authorization;
using ODK.Services;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups;

/// <summary>
/// Base class for all /my/groups/* pages
/// </summary>
[Authorize]
public abstract class OdkGroupAdminPageModel : OdkPageModel
{
    private readonly Lazy<MemberChapterAdminServiceRequest> _memberChapterAdminServiceRequest;

    protected OdkGroupAdminPageModel()
    {
        _memberChapterAdminServiceRequest = new(() =>
        MemberChapterAdminServiceRequest.Create(Securable, MemberChapterServiceRequest));
    }

    public MemberChapterAdminServiceRequest MemberChapterAdminServiceRequest => _memberChapterAdminServiceRequest.Value;

    public abstract ChapterAdminSecurable Securable { get; }
}