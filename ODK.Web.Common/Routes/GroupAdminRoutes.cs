using System;
using System.Collections.Generic;
using System.Linq;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Platforms;
using ODK.Services.Security;

namespace ODK.Web.Common.Routes;

public class GroupAdminRoutes
{
    /// <summary>
    /// Marks the members page as opening in bulk email mode. Bulk email is a mode of that page rather than
    /// a page of its own, so the route for it is the members page carrying this.
    /// </summary>
    public const string BulkEmailQueryKey = "email";

    public GroupAdminRoutes(PlatformType platform)
    {
        Platform = platform;
    }

    protected PlatformType Platform { get; }

    public GroupAdminRoute AdminMember(Chapter chapter, ChapterAdminMember adminMember)
        => AdminMembers(chapter).Child($"/{adminMember.MemberId}");

    public GroupAdminRoute AdminMembers(Chapter chapter)
        => Members(chapter).Child("/admins", ChapterAdminSecurable.AdminMembers);

    public GroupAdminRoute Conversation(Chapter chapter, Guid conversationId)
        => Conversations(chapter).Child($"/{conversationId}");

    public GroupAdminRoute Conversations(Chapter chapter)
        => Group(chapter).Child("/conversations", ChapterAdminSecurable.Conversations);

    public GroupAdminRoute Conversations(Chapter chapter, ChapterConversationStatus status)
        => Group(chapter).Child($"/conversations?status={status}", ChapterAdminSecurable.Conversations);

    public GroupAdminRoute Create() => Index().Child("/new");

    public GroupAdminRoute Delete(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/delete", ChapterAdminSecurable.Delete, PlatformType.Default)
    };

    public GroupAdminRoute Email(Chapter chapter, EmailType type) => Emails(chapter).Child($"/{type}");

    public GroupAdminRoute Emails(Chapter chapter)
        => Group(chapter).Child("/emails", ChapterAdminSecurable.Emails);

    public GroupAdminRoute Event(Chapter chapter, Guid eventId)
        => Events(chapter).Child($"/{eventId}");

    public GroupAdminRoute EventComments(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/comments");

    public GroupAdminRoute EventCreate(Chapter chapter)
        => Events(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute EventInvites(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/invites");

    public GroupAdminRoute EventResponses(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/responses");

    public GroupAdminRoute Events(Chapter chapter)
        => Base(chapter).Child("/events", ChapterAdminSecurable.Events);

    public GroupAdminRoute EventSettings(Chapter chapter)
        => Events(chapter).Child("/settings", ChapterAdminSecurable.EventSettings);

    public GroupAdminRoute EventTickets(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/tickets");

    public GroupAdminRoute Group(Chapter chapter) => Base(chapter).Child(Platform switch
    {
        PlatformType.DrunkenKnitwits => "/chapter",
        _ => ""
    });

    public GroupAdminRoute Home(Chapter chapter) => Events(chapter);

    public GroupAdminRoute Image(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/image", ChapterAdminSecurable.Branding, PlatformType.Default)
    };

    public GroupAdminRoute Import() => Index().Child("/import");

    public GroupAdminRoute Index() => Platform switch
    {
        // Member groups not implemented in DrunkenKnitwits platform
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => new()
        {
            Path = "/my/groups",
            Platform = PlatformType.Default,
            Securable = ChapterAdminSecurable.Any
        }
    };

    /// <summary>
    /// Where to send a member who has no specific destination — the admin landing page, and the
    /// fallback when a member is bounced off a page they cannot see. Prefers the events page: both
    /// platforms are events platforms, so anyone with elevated group privileges can reach it by
    /// definition. The menu-order fallback below is for the site admin with no chapter role, and for
    /// safety if that ever stops holding.
    /// </summary>
    /// <remarks>
    /// A fixed fallback route cannot be used here: a member who lacks access to it is redirected to
    /// it, bounced again, and loops. Returns null when the member may open no admin page at all —
    /// callers should treat that as not authorised rather than redirecting anywhere.
    /// </remarks>
    public GroupAdminRoute? LandingRoute(
        Chapter chapter, ChapterAdminMember? adminMember, Member currentMember)
    {
        var events = Events(chapter);
        if (events.IsPermitted(adminMember, currentMember, Platform))
        {
            return events;
        }

        foreach (var section in PermittedNavigation(chapter, adminMember, currentMember))
        {
            if (section.Route.IsPermitted(adminMember, currentMember, Platform))
            {
                return section.Route;
            }

            if (section.Items.Count > 0)
            {
                return section.Items.First().Route;
            }
        }

        return null;
    }

    public GroupAdminRoute Location(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/location", ChapterAdminSecurable.Location, PlatformType.Default)
    };

    public GroupAdminRoute Member(Chapter chapter, Guid memberId)
        => Members(chapter).Child($"/{memberId}");

    public GroupAdminRoute MemberApprovals(Chapter chapter)
        => Members(chapter).Child("/approvals", ChapterAdminSecurable.MemberApprovals, PlatformType.Default);

    public GroupAdminRoute MemberConversations(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/conversations", ChapterAdminSecurable.Conversations);

    public GroupAdminRoute MemberDelete(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/delete", ChapterAdminSecurable.MemberApprovals);

    public GroupAdminRoute MemberEvents(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/events");

    public GroupAdminRoute MemberImage(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/image", ChapterAdminSecurable.MemberImage, PlatformType.DrunkenKnitwits);

    public GroupAdminRoute MemberPayments(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/payments", ChapterAdminSecurable.Payments);

    public GroupAdminRoute MemberProperties(Chapter chapter)
        => Members(chapter).Child("/properties", ChapterAdminSecurable.Properties);

    public GroupAdminRoute MemberProperty(Chapter chapter, Guid propertyId)
        => MemberProperties(chapter).Child($"/{propertyId}");

    public GroupAdminRoute MemberPropertyCreate(Chapter chapter)
        => MemberProperties(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute MembersDownload(Chapter chapter)
        => new()
        {
            Path = $"/groups/{chapter.Id}/members/download",
            Securable = ChapterAdminSecurable.MemberExport
        };

    public GroupAdminRoute Members(Chapter chapter)
        => Base(chapter).Child("/members", ChapterAdminSecurable.Members);

    public GroupAdminRoute MembersEmail(Chapter chapter) =>
        Members(chapter).Child($"?{BulkEmailQueryKey}", ChapterAdminSecurable.BulkEmail);

    public GroupAdminRoute MembersImport(Chapter chapter)
        => Members(chapter).Child("/import", ChapterAdminSecurable.MemberImport);

    public GroupAdminRoute MembersImportTemplateDownload(Chapter chapter)
        => new()
        {
            Path = $"/groups/{chapter.Id}/members/import/template",
            Securable = ChapterAdminSecurable.MemberImport
        };

    /// <summary>
    /// Who the group has asked to join and is waiting on. Keyed to the import securable rather than to
    /// Members: an invitation exists only because an import raised one, and the people it lists are not
    /// members yet.
    /// </summary>
    public GroupAdminRoute MembersInvited(Chapter chapter)
        => Members(chapter).Child("/invited", ChapterAdminSecurable.MemberImport);

    public GroupAdminRoute MembershipSettings(Chapter chapter)
        => Base(chapter).Child("/membership", ChapterAdminSecurable.MembershipSettings);

    public GroupAdminRoute MembersSubscription(Chapter chapter, ChapterSubscription subscription)
        => Subscriptions(chapter).Child($"/{subscription.Id}");

    public GroupAdminRoute MembersSubscriptionCreate(Chapter chapter)
        => Subscriptions(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute Message(Chapter chapter, Guid messageId)
        => Messages(chapter).Child($"/{messageId}");

    public GroupAdminRoute Messages(Chapter chapter)
        => Group(chapter).Child("/messages", ChapterAdminSecurable.ContactMessages);

    public GroupAdminRoute Messages(Chapter chapter, MessageStatus status)
        => Messages(chapter).Child($"?status={status}");

    /// <summary>
    /// The full group admin menu tree, before any permission or platform filtering. This is the single
    /// definition of what the admin area contains; the side menu and the admin landing redirect both
    /// derive from it, so a new admin page is registered once here rather than in each consumer.
    /// </summary>
    public IReadOnlyCollection<GroupAdminNavSection> Navigation(Chapter chapter) =>
    [
        new GroupAdminNavSection
        {
            Route = Group(chapter),
            Text = "Group",
            Items =
            [
                new(Conversations(chapter), "Conversations"),
                new(Emails(chapter), "Emails"),
                new(Questions(chapter), "FAQ"),
                new(Location(chapter), "Location"),
                new(Messages(chapter), "Messages"),
                new(Image(chapter), "Picture"),
                new(Pages(chapter), "Pages"),
                new(Privacy(chapter), "Privacy"),
                new(SocialMedia(chapter), "Social media"),
                new(Subscription(chapter), "Subscription"),
                new(Texts(chapter), "Texts"),
                new(Theme(chapter), "Theme"),
                new(Topics(chapter), "Topics"),
                new(Delete(chapter), "Delete")
            ]
        },
        new GroupAdminNavSection
        {
            Route = Events(chapter),
            Text = "Events",
            Items =
            [
                new(Venues(chapter), "Venues"),
                new(EventSettings(chapter), "Settings")
            ]
        },
        new GroupAdminNavSection
        {
            Route = Members(chapter),
            Text = "Members",
            Items =
            [
                new(AdminMembers(chapter), "Admins"),
                new(MemberProperties(chapter), "Profile questions"),
                new(MembersEmail(chapter), "Bulk email"),
                new(MemberApprovals(chapter), "Approvals"),
                new(MembersImport(chapter), "Import"),
                new(MembersInvited(chapter), "Invited")
            ]
        },
        new GroupAdminNavSection
        {
            Route = MembershipSettings(chapter),
            Text = "Membership",
            Items =
            [
                new(Subscriptions(chapter), "Subscriptions")
            ]
        },
        new GroupAdminNavSection
        {
            Route = Payments(chapter),
            Text = "Payments",
            Items =
            [
                new(PaymentAccount(chapter), "Account")
            ]
        },
        new GroupAdminNavSection
        {
            RequiresSiteAdmin = true,
            Route = SiteAdmin(chapter),
            Text = "Site Admin",
            Items =
            [
                new(SiteAdminMembers(chapter), "Members"),
                new(SiteAdminPayments(chapter), "Payments"),
                new(SiteAdminSubscriptions(chapter), "Subscriptions"),
                new(SiteAdminLocation(chapter), "Location"),
                new(SiteAdminInstagram(chapter), "Instagram"),
                new(SiteAdminRedirect(chapter), "Redirect"),
                new(SiteAdminTheme(chapter), "Theme")
            ]
        }
    ];

    public GroupAdminRoute Pages(Chapter chapter)
        => Group(chapter).Child("/pages", ChapterAdminSecurable.Pages, PlatformType.Default);

    public GroupAdminRoute PaymentAccount(Chapter chapter)
        => Payments(chapter).Child("/account", ChapterAdminSecurable.PaymentAccount);

    public GroupAdminRoute Payments(Chapter chapter)
        => Base(chapter).Child("/payments", ChapterAdminSecurable.Payments);

    /// <summary>
    /// <see cref="Navigation"/> reduced to what this member may open on this platform. A section is
    /// dropped when neither it nor any of its items survives, so the result is safe to render directly
    /// and safe to pick a redirect target from.
    /// </summary>
    public IReadOnlyCollection<GroupAdminNavSection> PermittedNavigation(
        Chapter chapter, ChapterAdminMember? adminMember, Member currentMember)
    {
        var permitted = new List<GroupAdminNavSection>();

        foreach (var section in Navigation(chapter))
        {
            if (section.RequiresSiteAdmin && !currentMember.SiteAdmin)
            {
                continue;
            }

            var items = section.Items
                .Where(x => x.Route.IsPermitted(adminMember, currentMember, Platform))
                .ToArray();

            var sectionPermitted = section.Route.IsPermitted(adminMember, currentMember, Platform);
            if (!sectionPermitted && items.Length == 0)
            {
                continue;
            }

            permitted.Add(new GroupAdminNavSection
            {
                Items = items,
                RequiresSiteAdmin = section.RequiresSiteAdmin,
                Route = section.Route,
                Text = section.Text
            });
        }

        return permitted;
    }

    public GroupAdminRoute Privacy(Chapter chapter)
        => Group(chapter).Child("/privacy", ChapterAdminSecurable.PrivacySettings);

    public GroupAdminRoute Question(Chapter chapter, Guid questionId)
        => Questions(chapter).Child($"/{questionId}");

    public GroupAdminRoute QuestionCreate(Chapter chapter)
        => Questions(chapter).Child(Platform == PlatformType.DrunkenKnitwits ? "/create" : "/new");

    public GroupAdminRoute Questions(Chapter chapter)
        => Group(chapter).Child("/questions", ChapterAdminSecurable.Questions);

    public GroupAdminRoute SiteAdmin(Chapter chapter)
        => Base(chapter).Child("/siteadmin");

    public GroupAdminRoute SiteAdminInstagram(Chapter chapter)
        => SiteAdmin(chapter).Child("/instagram", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminLocation(Chapter chapter)
        => SiteAdmin(chapter).Child("/location", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminMembers(Chapter chapter)
        => SiteAdmin(chapter).Child("/members", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminPayments(Chapter chapter)
        => SiteAdmin(chapter).Child("/payments", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminRedirect(Chapter chapter)
        => SiteAdmin(chapter).Child("/redirect", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminSubscriptions(Chapter chapter)
        => SiteAdmin(chapter).Child("/subscriptions", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SiteAdminTheme(Chapter chapter)
        => SiteAdmin(chapter).Child("/theme", platform: PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SocialMedia(Chapter chapter)
        => Group(chapter).Child("/social-media", ChapterAdminSecurable.SocialMedia);

    public GroupAdminRoute Subscription(Chapter chapter)
        => Group(chapter).Child("/subscription", ChapterAdminSecurable.SiteSubscription, PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SubscriptionCheckout(Chapter chapter, Guid priceId)
        => Subscription(chapter).Child($"/{priceId}/checkout");

    public GroupAdminRoute SubscriptionConfirm(Chapter chapter)
        => Subscription(chapter).Child("/confirm?sessionId={sessionId}");

    public GroupAdminRoute Subscriptions(Chapter chapter)
        => MembershipSettings(chapter).Child("/subscriptions", ChapterAdminSecurable.Subscriptions);

    public GroupAdminRoute Texts(Chapter chapter)
        => Group(chapter).Child("/texts", ChapterAdminSecurable.Texts);

    public GroupAdminRoute Theme(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/theme", ChapterAdminSecurable.Branding, PlatformType.Default)
    };

    public GroupAdminRoute Topics(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/topics", ChapterAdminSecurable.Topics, PlatformType.Default)
    };

    public GroupAdminRoute Venue(Chapter chapter, Guid venueId) =>
        Venues(chapter).Child($"/{venueId}");

    public GroupAdminRoute VenueCreate(Chapter chapter) =>
        Venues(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute VenueEvents(Chapter chapter, Guid venueId)
        => Venue(chapter, venueId).Child("/events");

    public GroupAdminRoute Venues(Chapter chapter) => Venues(chapter, archived: false);

    public GroupAdminRoute Venues(Chapter chapter, bool archived)
        => Events(chapter).Child($"/venues{(archived ? "?archived=true" : null)}", ChapterAdminSecurable.Venues);

    private GroupAdminRoute Base(Chapter chapter) => new()
    {
        Path = Platform switch
        {
            PlatformType.DrunkenKnitwits => $"/{chapter.ShortName.ToLowerInvariant()}/admin",
            _ => Index().Child($"/{chapter.Id}").Path
        },
        Securable = ChapterAdminSecurable.Any
    };
}