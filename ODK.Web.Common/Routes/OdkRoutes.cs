namespace ODK.Web.Common.Routes;

public static class OdkRoutes
{
    public static AccountRoutes Account { get; } = new AccountRoutes();
    public static GroupRoutes Groups { get; } = new GroupRoutes();

    public static MemberGroupRoutes MemberGroups { get; } = new MemberGroupRoutes();
    public static MemberRoutes Members { get; } = new MemberRoutes();
}
