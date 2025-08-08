using ODK.Core.Emails;

namespace ODK.Services.Emails;

public interface IEmailClientFactory
{
    IEmailClient Create(EmailProviderType type);
}
