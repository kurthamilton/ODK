namespace ODK.Services.Integrations.Emails.Brevo.Models;

public class BrevoEmailAddressee
{
    public required string Email { get; init; }

    public string? Name { get; init; }
}
