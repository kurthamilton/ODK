using System;

namespace ODK.Web.Common.Routes;

public class MemberGroupRoutes
{
    public string Create() => $"{Index()}/new";
    
    public string Group(Guid id) => $"{Index()}/{id}";

    public string Index() => "/my/groups";
}
