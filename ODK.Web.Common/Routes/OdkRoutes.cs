using System;
using ODK.Core.Chapters;
using ODK.Web.Common.Services;

namespace ODK.Web.Common.Routes;

/// <summary>
/// Only to be injected into UI classes, otherwise instances should be created
/// from the <see cref="OdkRoutesFactory"/>
/// </summary>
public class OdkRoutes : IOdkRoutes
{
    private readonly Lazy<AccountRoutes> _accountRoutes;
    private readonly Lazy<MemberGroupRoutes> _groupAdminRoutes;
    private readonly Lazy<GroupRoutes> _groupRoutes;
    private readonly Lazy<MemberRoutes> _memberRoutes;
    private readonly Lazy<PaymentsRoutes> _paymentsRoutes;
    private readonly Lazy<SiteAdminRoutes> _siteAdminRoutes;

    private readonly IRequestStore _requestStore;

    public OdkRoutes(IRequestStore requestStore)
    {
        _requestStore = requestStore;

        _accountRoutes = new(() => new AccountRoutes(_requestStore.Platform));
        _groupAdminRoutes = new(() => new MemberGroupRoutes(_requestStore.Platform));
        _groupRoutes = new(() => new GroupRoutes(Account, _requestStore.Platform));
        _memberRoutes = new(() => new MemberRoutes());
        _paymentsRoutes = new(() => new PaymentsRoutes(_requestStore.Platform));
        _siteAdminRoutes = new(() => new SiteAdminRoutes());
    }

    public AccountRoutes Account => _accountRoutes.Value;
    public MemberGroupRoutes GroupAdmin => _groupAdminRoutes.Value;
    public GroupRoutes Groups => _groupRoutes.Value;
    public MemberRoutes Members => _memberRoutes.Value;
    public PaymentsRoutes Payments => _paymentsRoutes.Value;
    public SiteAdminRoutes SiteAdmin => _siteAdminRoutes.Value;

    public string Error(Chapter? chapter, int statusCode)
        => chapter != null
            ? Groups.Error(chapter, statusCode)
            : $"/error/{statusCode}";
}