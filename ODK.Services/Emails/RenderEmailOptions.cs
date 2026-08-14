using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

/// <summary>
/// Everything needed to resolve an email's wording. <see cref="SendEmailOptions"/> inherits this and adds
/// what only a send needs, so a preview and the send it previews cannot resolve differently.
/// </summary>
public class RenderEmailOptions
{
    public required string Body { get; init; }

    public Chapter? Chapter { get; init; }

    /// <summary>
    /// The layout to wrap <see cref="Body"/> in, for a caller holding one that is not what is stored -
    /// previewing an edited layout template. Null takes the group's layout, or the site's.
    /// </summary>
    public string? Layout { get; init; }

    public IEmailParameters? Parameters { get; init; }

    /// <summary>
    /// Who the email is written for, for a send that carries its own subject and body and so has no email
    /// row to declare it. Left unset where <see cref="Type"/> names an email, whose own
    /// <see cref="Email.RecipientType"/> answers it instead.
    /// </summary>
    public EmailRecipientType RecipientType { get; init; }

    public required string Subject { get; init; }

    public EmailType Type { get; init; } = EmailType.Layout;
}
