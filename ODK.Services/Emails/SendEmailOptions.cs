using ODK.Core.Emails;

namespace ODK.Services.Emails;

public class SendEmailOptions : RenderEmailOptions
{
    public required IReadOnlyCollection<EmailAddressee> To { get; init; }
}
