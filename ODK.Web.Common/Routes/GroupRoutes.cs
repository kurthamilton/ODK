using System.Web;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{    
    public string Group(string slug) => $"/groups/{HttpUtility.UrlEncode(slug.ToLowerInvariant())}";    
}
