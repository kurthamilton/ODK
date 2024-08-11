using System.Web;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{    
    public string Group(string slug) => $"{Index()}/{HttpUtility.UrlEncode(slug.ToLowerInvariant())}";

    public string Index() => "/groups";
}
