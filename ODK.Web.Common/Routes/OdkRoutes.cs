namespace ODK.Web.Common.Routes;

public static class OdkRoutes
{
    public static AccountRoutes Account { get; } = new AccountRoutes();
    public static MemberRoutes Members { get; } = new MemberRoutes();
}
