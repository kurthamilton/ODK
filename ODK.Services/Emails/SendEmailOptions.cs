using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

public class SendEmailOptions
{
    public required string Body { get; init; }

    public Chapter? Chapter { get; init; }

    public IDictionary<string, string>? Parameters { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyCollection<EmailAddressee> To { get; init; }

    public EmailType Type { get; init; } = EmailType.Layout;
}
