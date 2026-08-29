namespace ODK.Services.Emails;

/// <summary>
/// A Brevo transactional email event this deployment is to act on. It carries no environment: one exists only
/// once the environment has been checked, so exposing it would invite a second check.
/// </summary>
public class BrevoWebhookEvent
{
    public required string EventName { get; init; }

    public required string ExternalId { get; init; }
}
