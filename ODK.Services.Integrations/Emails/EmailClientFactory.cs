using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Smtp;

namespace ODK.Services.Integrations.Emails;

public class EmailClientFactory : IEmailClientFactory
{
    private readonly SmtpEmailClient _brevo;

    public EmailClientFactory(SmtpEmailClient brevo)
    {
        _brevo = brevo;
    }

    public IEmailClient Create(EmailProviderType type)
    {
        return type switch
        {
            EmailProviderType.Smtp => _brevo,
            _ => throw new Exception($"EmailProvider '{type}' not supported")
        };
    }
}
