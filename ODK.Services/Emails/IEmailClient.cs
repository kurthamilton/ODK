using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailClient
{
    Task<SendEmailResult> SendEmail(EmailProvider provider, EmailClientEmail email);
}
