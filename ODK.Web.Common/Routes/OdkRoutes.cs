using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Web.Common.Services;

namespace ODK.Web.Common.Routes;

/// <summary>
/// Only to be injected into UI classes, otherwise instances should be created
/// from the <see cref="OdkRoutesFactory"/>
/// </summary>
public class OdkRoutes : IOdkRoutes
{
    private readonly Lazy<AccountRoutes> _accountRoutes;
    private readonly Lazy<GroupAdminRoutes> _groupAdminRoutes;
    private readonly Lazy<GroupRoutes> _groupRoutes;
    private readonly Lazy<MemberRoutes> _memberRoutes;
    private readonly Lazy<PaymentRoutes> _paymentRoutes;
    private readonly Lazy<PublicRoutes> _publicRoutes;
    private readonly Lazy<SiteAdminRoutes> _siteAdminRoutes;
    private readonly Lazy<SiteRoutes> _siteRoutes;

    /// <summary>
    /// Used by dependency injection. Platform may not be known at the time of dependency injection,
    /// so lazy resolve the platform.
    /// </summary>
    public OdkRoutes(IRequestStore requestStore)
        : this(() => requestStore.Platform)
    {
    }

    /// <summary>
    /// Used when the platform is already known
    /// </summary>
    public OdkRoutes(PlatformType platform)
        : this(() => platform)
    {
    }

    private OdkRoutes(Func<PlatformType> platformFactory)
    {
        _accountRoutes = new(() => new AccountRoutes(platformFactory()));
        _groupAdminRoutes = new(() => new GroupAdminRoutes(platformFactory()));
        _groupRoutes = new(() => new GroupRoutes(Account, platformFactory()));
        _memberRoutes = new(() => new MemberRoutes());
        _paymentRoutes = new(() => new PaymentRoutes());
        _publicRoutes = new(() => new PublicRoutes(Groups, Site, platformFactory()));
        _siteAdminRoutes = new(() => new SiteAdminRoutes());
        _siteRoutes = new(() => new SiteRoutes());
    }

    public AccountRoutes Account => _accountRoutes.Value;
    public GroupAdminRoutes GroupAdmin => _groupAdminRoutes.Value;
    public GroupRoutes Groups => _groupRoutes.Value;
    public MemberRoutes Members => _memberRoutes.Value;
    public PaymentRoutes Payments => _paymentRoutes.Value;
    public PublicRoutes Public => _publicRoutes.Value;
    public SiteRoutes Site => _siteRoutes.Value;
    public SiteAdminRoutes SiteAdmin => _siteAdminRoutes.Value;

    public string Error(Chapter? chapter, int statusCode)
        => chapter != null && chapter.IsPublished()
            ? Groups.Error(chapter, statusCode)
            : $"/error/{statusCode}";
}