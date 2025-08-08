using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailClient
{
    Task<ServiceResult> SendEmail(EmailProvider provider, EmailClientEmail email);
}
