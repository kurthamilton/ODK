using System;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Messages;

namespace ODK.Web.Common.Routes;

public class SiteAdminRoutes
{
    public string Emails => Path("/emails");

    public string Errors => Path("/errors");

    public string FeatureCreate => $"{Features}/create";

    public string Features => Path("/features");

    public string Groups => Path("/groups");

    public string Impersonate => Path("/impersonate");

    public string Index => "/siteadmin";

    public string Issues => Path("/issues");

    public string PaymentCreate => $"{Payments}/new";

    public string Payments => Path("/payments");

    public string SubscriptionCreate => $"{Subscriptions}/new";

    public string Subscriptions => Path("/subscriptions");

    public string Topics => Path("/topics");

    public string Email(EmailType type) => $"{Emails}/{type}";

    public string Error(Guid id) => $"{Errors}/{id}";

    public string Feature(Guid id) => $"{Features}/{id}";

    public string Group(Guid id) => $"{Groups}/{id}";

    public string Issue(Guid id) => $"{Issues}/{id}";

    public string Message(Guid id) => $"{Messages()}/{id}";

    public string Messages() => Path($"/messages");

    public string Messages(MessageStatus status) => $"{Messages()}?status={status}";

    public string Payment(Guid id) => $"{Payments}/{id}";

    public string Subscription(Guid id) => $"{Subscriptions}/{id}";

    public string Topic(Guid id) => $"{Topics}/{id}";

    private string Path(string subPath) => Index + subPath;
}