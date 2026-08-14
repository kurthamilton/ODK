using System.Globalization;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Web;

namespace ODK.Services.Emails.Parameters;

public class TestEmailParametersFactory : ITestEmailParametersFactory
{
    /* Upper case on purpose: a stand-in value has to be recognisable as one at a glance, so nobody reads a
       preview and takes it for the group's own content. */
    private const string EventName = "EVENT NAME";

    private const string VenueName = "VENUE NAME";

    private readonly IUrlProviderFactory _urlProviderFactory;

    public TestEmailParametersFactory(IUrlProviderFactory urlProviderFactory)
    {
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<IEmailParameters> Create(
        IServiceRequest request, EmailType type, Member member, CultureInfo culture, Chapter? chapter)
    {
        var urlProvider = await _urlProviderFactory.Create(request);

        return new CombinedEmailParameters(
            GroupParameters(request, urlProvider, chapter),
            TypeParameters(urlProvider, type, member, culture, chapter));
    }

    /* Supplied here as well as by EmailService, which resolves the same three from the chapter it is
       sending for. The two are not the same chapter: a test email has to keep being looked up against
       the template the admin is editing, so the stand-in group reaches the email as parameters and never
       as the chapter to send as - passing it as the latter would fetch that group's override instead of
       the template under test. Merged over the core values, so these win where a group was resolved. */
    private static IEmailParameters? GroupParameters(
        IServiceRequest request, IUrlProvider urlProvider, Chapter? chapter)
        => chapter != null
            ? new EmailParameters
            {
                GroupName = chapter.FullName,
                GroupUrl = urlProvider.GroupUrl(chapter)
            }
            : null;

    /* A whole event rather than a bare one, so every event parameter resolves to something readable. An
       unfilled entity leaves event.name empty and dates it to DateTime.MinValue, which reads as a broken
       template rather than as a template with nothing filled in. */
    private static Event TestEvent() => new()
    {
        // A clean hour next week, rather than now: an arbitrary minute reads like real data.
        DateUtc = DateTime.UtcNow.Date.AddDays(7).AddHours(19),
        EndTime = new TimeSpan(21, 0, 0),
        Name = EventName,
        // Published, or GetDisplayName prefixes "[DRAFT] " onto the name in every preview.
        PublishedUtc = DateTime.UtcNow
    };

    private static Venue TestVenue() => new()
    {
        Name = VenueName
    };

    /* Only the types with a parameter this can answer. Null for the rest - there is nothing to add, and
       an empty set of the right class would say the same thing at more length. A type gains an entry here
       when one of its parameters becomes answerable from the member or the group alone. */
    private static IEmailParameters? TypeParameters(
        IUrlProvider urlProvider, EmailType type, Member member, CultureInfo culture, Chapter? chapter)
        => type switch
        {
            EmailType.ActivateAccount => new ActivateAccountParameters
            {
                Url = urlProvider.ActivateAccountUrl(chapter, "TEST")
            },
            EmailType.ContactRequest => new ContactRequestParameters
            {
                From = "test@email.com",
                Text = "Test contact message",
                Url = chapter != null
                    ? urlProvider.MessageAdminUrl(chapter, Guid.Empty)
                    : urlProvider.MessageSiteAdminUrl(Guid.Empty)
            },
            EmailType.DuplicateEmail => new DuplicateEmailParameters
            {
                LoginUrl = urlProvider.LoginUrl(chapter)
            },
            EmailType.EmailAddressUpdate => new EmailAddressUpdateParameters
            {
                Url = urlProvider.ConfirmEmailAddressUpdate(chapter, "TEST")
            },
            EmailType.EventComment or
            EmailType.EventCommentReply => chapter != null
                ? new EventCommentParameters(TestEvent())
                {
                    EventUrl = urlProvider.EventUrl(chapter, "TEST"),
                    Text = "Test comment"
                }
                : null,
            EmailType.EventInvite => chapter != null
                ? new EventInviteParameters(chapter, TestEvent(), TestVenue(), culture)
                {
                    RsvpUrl = urlProvider.EventRsvpUrl(chapter, "TEST"),
                    UnsubscribeUrl = urlProvider.EmailPreferences(chapter),
                    Url = urlProvider.EventUrl(chapter, "TEST")
                }
                : null,
            EmailType.MemberImportActivation => new MemberImportActivationParameters
            {
                Url = urlProvider.ActivateAccountUrl(chapter, "TEST")
            },
            EmailType.MemberImportInvite => new MemberImportInviteParameters
            {
                Url = urlProvider.JoinUrl()
            },
            EmailType.NewMember => chapter != null
                ? new NewMemberParameters
                {
                    EventsUrl = urlProvider.EventsUrl(chapter),
                    FirstName = member.FirstName
                }
                : null,
            EmailType.NewMemberAdmin => chapter != null
                ? new NewMemberAdminParameters
                {
                    AdminUrl = urlProvider.MemberAdminUrl(chapter, Guid.Empty),
                    PropertiesHtml = new EmailTableBuilder()
                        .AddRow("Name", member.FullName)
                        .AddRow("Test property", "Test value")
                        .ToString()
                }
                : null,
            EmailType.PasswordReset => new PasswordResetParameters
            {
                Url = urlProvider.PasswordReset(chapter, "TEST")
            },
            EmailType.SubscriptionConfirmation => new SubscriptionConfirmationParameters(
                new Currency { Symbol = "X" }, member, culture)
            {
                Amount = 1.23M,
                ExpiresUtc = DateTime.UtcNow.AddMonths(1)
            },
            EmailType.SubscriptionExpired or
            EmailType.SubscriptionExpiring or
            EmailType.TrialExpired or
            EmailType.TrialExpiring => new SubscriptionExpiryParameters(member, culture)
            {
                DisabledUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow
            },
            _ => null
        };
}
