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

    public SiteAdminRoute Members => Path("/members");

    public SiteAdminRoute MembersFlagged => Members.Child("/flagged");

    /// <summary>
    /// The payments waiting on something the payment provider has yet to be asked for, and the action that
    /// asks. Its own page rather than part of <see cref="Payments"/>: it lists rows, and the action on it
    /// reaches out to every account.
    /// </summary>
    public SiteAdminRoute PaymentReconciliation => Payments.Child("/reconciliation");

    /// <summary>
    /// The form for writing down a refund. Takes an optional <c>paymentId</c>, which fills it in - the
    /// route a group's payments page links to, so a site admin does not have to carry a charge id across.
    /// </summary>
    public SiteAdminRoute PaymentRefunds => Payments.Child("/refunds");

    public SiteAdminRoute Payments => Path("/payments");

    /// <summary>
    /// Everything the platform's Stripe account holds, against the records that should account for it. Its
    /// own page rather than part of <see cref="Payments"/>, which lists our payments alone: rendering this
    /// sweeps the whole account.
    /// </summary>
    public SiteAdminRoute PaymentTransactions => Payments.Child("/transactions");

    /// <summary>
    /// The webhook endpoints registered against the platform's payment provider accounts. Its own page
    /// rather than part of <see cref="Payments"/>: rendering it calls out to every account.
    /// </summary>
    public SiteAdminRoute PaymentWebhooks => Payments.Child("/webhooks");

    public SiteAdminRoute QuestionCreate => Questions.Child("/new");

    public SiteAdminRoute Questions => Path("/questions");

    public SiteAdminRoute ReferralCampaignCreate => Referrals.Child("/new");

    public SiteAdminRoute Referrals => Path("/referrals");

    public SiteAdminRoute SubscriptionCreate => Subscriptions.Child("/new");

    public SiteAdminRoute Subscriptions => Path("/subscriptions");

    public SiteAdminRoute Topics => Path("/topics");

    public SiteAdminRoute Workflows => Path("/workflows");

    /// <summary>
    /// A member's thread with the site's admins. Distinct from <see cref="Messages()"/>, which is where
    /// anonymous contact lands - that carries a name, an address and a spam status because anybody can send
    /// one; these come from a member who is signed in.
    /// </summary>
    public SiteAdminRoute Conversation(Guid id) => Conversations().Child($"/{id}");

    public SiteAdminRoute Conversations() => Path("/conversations");

    public SiteAdminRoute Conversations(bool archived)
        => archived ? Conversations().Child("?archived=true") : Conversations();

    public SiteAdminRoute Country(Guid id) => Countries.Child($"/{id}");

    public SiteAdminRoute Email(EmailType type) => Emails.Child($"/{type}");

    public SiteAdminRoute Error(Guid id) => Errors.Child($"/{id}");

    public SiteAdminRoute Feature(Guid id) => Features.Child($"/{id}");

    public SiteAdminRoute Group(Guid id) => Groups.Child($"/{id}");

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
        new(Conversations(), "Conversations"),
        new(Countries, "Countries"),
        new(Emails, "Emails"),
        new(Errors, "Error log"),
        new(Questions, "FAQ"),
        new(Features, "Features"),
        new(Groups, "Groups"),
        new(Impersonate, "Impersonate"),
        new(Members, "Members")
        {
            Children = [new(MembersFlagged, "Flagged")]
        },
        new(Messages(), "Messages"),
        new(Payments, "Payments")
        {
            Children =
            [
                new(PaymentReconciliation, "Reconciliation"),
                new(PaymentRefunds, "Refunds"),
                new(PaymentTransactions, "Transactions"),
                new(PaymentWebhooks, "Webhooks")
            ]
        },
        new(Referrals, "Referrals"),
        new(Subscriptions, "Subscriptions"),
        new(Topics, "Topics"),
        new(Workflows, "Workflows")
    ];

    public SiteAdminRoute Question(Guid id) => Questions.Child($"/{id}");

    public SiteAdminRoute ReferralCampaign(Guid id) => Referrals.Child($"/{id}");

    public SiteAdminRoute Subscription(Guid id) => Subscriptions.Child($"/{id}");

    public SiteAdminRoute Topic(Guid id) => Topics.Child($"/{id}");

    private SiteAdminRoute Path(string subPath) => Index.Child(subPath);
}
