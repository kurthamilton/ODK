using ODK.Core.Utils;

namespace ODK.Core.Emails;

public class ChapterEmail : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string HtmlContent { get; set; } = string.Empty;

    public Guid Id { get; set; }

    /// <inheritdoc cref="Email.Name" />
    public string Name => EnumUtils.GetDisplayValue(Type);

    public string Subject { get; set; } = string.Empty;

    public EmailType Type { get; set; }

    public bool IsDefault() => Id == default;

    /// <summary>
    /// The group's override as the email it stands in for.
    /// </summary>
    /// <param name="recipientType">
    /// Taken from the site's row for this type, because an override customises the wording and not who the
    /// email is for - there is no column here for a group to disagree about it. Passed in rather than
    /// defaulted so that it cannot silently arrive as <see cref="EmailRecipientType.None"/>.
    /// </param>
    public Email ToEmail(EmailRecipientType recipientType)
    {
        return new Email
        {
            HtmlContent = HtmlContent,
            RecipientType = recipientType,
            Subject = Subject,
            Type = Type
        };
    }
}
