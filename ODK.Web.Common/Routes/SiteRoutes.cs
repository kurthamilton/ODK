namespace ODK.Web.Common.Routes;

public class SiteRoutes
{
    public string About => "/about";

    public string Contact => "/contact";

    /// <summary>
    /// Renders feedback toasts from the values a request states, for a script showing the result of a post it
    /// made itself.
    /// </summary>
    public string Feedback => "/feedback";

    public string Pricing => "/pricing";

    public string Privacy => "/privacy";

    /// <summary>The refer-a-friend page. Not available on the DrunkenKnitwits platform.</summary>
    public string Refer => "/refer";
}
