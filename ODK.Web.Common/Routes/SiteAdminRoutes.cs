using System;
using ODK.Core.Emails;

namespace ODK.Web.Common.Routes;

public class SiteAdminRoutes
{
    public string Email(EmailType type) => $"{Emails}/{type}";

    public string Emails => Path("/emails");

    public string Error(Guid id) => $"{Errors}/{id}";

    public string Errors => Path("/errors");

    public string Feature(Guid id) => $"{Features}/{id}";

    public string FeatureCreate => $"{Features}/create";

    public string Features => Path("/features");

    public string Group(Guid id) => $"{Groups}/{id}";

    public string Groups => Path("/groups");

    public string Index => "/siteadmin";

    public string Issue(Guid id) => $"{Issues}/{id}";

    public string Issues => Path("/issues");

    public string Message(Guid id) => $"{Messages}/{id}";

    public string Messages => Path("/messages");

    public string Payment(Guid id) => $"{Payments}/{id}";

    public string PaymentCreate => $"{Payments}/new";

    public string Payments => Path("/payments");

    public string Subscription(Guid id) => $"{Subscriptions}/{id}";

    public string SubscriptionCreate => $"{Subscriptions}/new";

    public string Subscriptions => Path("/subscriptions");

    public string Topics => Path("/topics");

    private string Path(string subPath) => Index + subPath;
}