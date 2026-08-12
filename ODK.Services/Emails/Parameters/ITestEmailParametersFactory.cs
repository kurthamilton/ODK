using System.Globalization;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Services.Emails.Parameters;

public interface ITestEmailParametersFactory
{
    /// <summary>
    /// The parameters to send a test email with, so an admin checking a template sees it rendered against
    /// real values rather than tokens. Covers the parameters that can be answered from who is asking and
    /// which group they are asking about - the member's own details and the group's name and URLs.
    /// Anything needing a subject of its own (an event, a subscription, a one-time token) is left unset,
    /// which leaves its token visible in the email rather than blanking it.
    /// </summary>
    /// <param name="chapter">
    /// The group to describe, which is not necessarily the group whose template is being sent - a site
    /// admin has no current group, so a group of theirs stands in for one. Null when they belong to none.
    /// </param>
    Task<IEmailParameters> Create(
        IServiceRequest request, EmailType type, Member member, CultureInfo culture, Chapter? chapter);
}
