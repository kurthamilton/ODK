using Microsoft.AspNetCore.Authorization;
using ODK.Services;
using ODK.Services.Security;
using MemberChapterAdminServiceRequestImpl = ODK.Services.MemberChapterAdminServiceRequest;

namespace ODK.Web.Razor.Pages.My.Groups;

/// <summary>
/// Base class for all /my/groups/* pages
/// </summary>
[Authorize]
public abstract class OdkGroupAdminPageModel : OdkPageModel
{
    private readonly Lazy<IMemberChapterAdminServiceRequest> _memberChapterAdminServiceRequest;

    protected OdkGroupAdminPageModel()
    {
        _memberChapterAdminServiceRequest = new(() =>
        MemberChapterAdminServiceRequestImpl.Create(Securable, MemberChapterServiceRequest));
    }

    public IMemberChapterAdminServiceRequest MemberChapterAdminServiceRequest => _memberChapterAdminServiceRequest.Value;

    public abstract ChapterAdminSecurable Securable { get; }
}