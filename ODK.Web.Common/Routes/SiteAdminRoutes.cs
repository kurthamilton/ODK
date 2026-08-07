using System;
using System.Collections.Generic;
using ODK.Core.Emails;
using ODK.Core.Messages;

namespace ODK.Web.Common.Routes;

public class SiteAdminRoutes
{
    public SiteAdminRoute Countries => Path("/countries");

    public SiteAdminRoute Emails => Path("/emails");

    public SiteAdminRoute Errors => Path("/errors");

    public SiteAdminRoute FeatureCreate => Features.Child("/create");

    public SiteAdminRoute Features => Path("/features");

    public SiteAdminRoute Groups => Path("/groups");

    public SiteAdminRoute Impersonate => Path("/impersonate");

    public SiteAdminRoute Index => new("/siteadmin");

    public SiteAdminRoute Issues => Path("/issues");

    public SiteAdminRoute Members => Path("/members");

    public SiteAdminRoute MembersFlagged => Members.Child("/flagged");

    public SiteAdminRoute PaymentCreate => Payments.Child("/new");

    public SiteAdminRoute Payments => Path("/payments");

    public SiteAdminRoute SubscriptionCreate => Subscriptions.Child("/new");

    public SiteAdminRoute Subscriptions => Path("/subscriptions");

    public SiteAdminRoute Topics => Path("/topics");

    public SiteAdminRoute Country(Guid id) => Countries.Child($"/{id}");

    public SiteAdminRoute Email(EmailType type) => Emails.Child($"/{type}");

    public SiteAdminRoute Error(Guid id) => Errors.Child($"/{id}");

    public SiteAdminRoute Feature(Guid id) => Features.Child($"/{id}");

    public SiteAdminRoute Group(Guid id) => Groups.Child($"/{id}");

    public SiteAdminRoute Issue(Guid id) => Issues.Child($"/{id}");

    public SiteAdminRoute Message(Guid id) => Messages().Child($"/{id}");

    public SiteAdminRoute Messages() => Path("/messages");

    public SiteAdminRoute Messages(MessageStatus status) => Messages().Child($"?status={status}");

    /// <summary>
    /// The site admin menu, defined alongside the routes themselves so the layout does not repeat them.
    /// Site admin access is all-or-nothing, so unlike the group admin equivalent there is nothing to
    /// filter — a member either reaches every one of these or none.
    /// </summary>
    public IReadOnlyCollection<SiteAdminNavItem> Navigation() =>
    [
        new(Countries, "Countries"),
        new(Emails, "Emails"),
        new(Errors, "Error log"),
        new(Features, "Features"),
        new(Groups, "Groups"),
        new(Impersonate, "Impersonate"),
        new(Issues, "Issues"),
        new(Members, "Members")
        {
            Children = [new(MembersFlagged, "Flagged")]
        },
        new(Messages(), "Messages"),
        new(Payments, "Payments"),
        new(Subscriptions, "Subscriptions"),
        new(Topics, "Topics")
    ];

    public SiteAdminRoute Payment(Guid id) => Payments.Child($"/{id}");

    public SiteAdminRoute Subscription(Guid id) => Subscriptions.Child($"/{id}");

    public SiteAdminRoute Topic(Guid id) => Topics.Child($"/{id}");

    private SiteAdminRoute Path(string subPath) => Index.Child(subPath);
}
