using ODK.Core.Emails;

namespace ODK.Services.Integrations.Emails.Brevo.Models;

public class BrevoEmailAddressee
{
    public BrevoEmailAddressee(string email)
    {
        Email = email;
    }

    public BrevoEmailAddressee(EmailAddressee addressee)
        : this(addressee.Address)
    {
        Name = !string.IsNullOrEmpty(addressee.Name)
            ? addressee.Name
            : null;
    }

    public string Email { get; }

    public string? Name { get; }
}