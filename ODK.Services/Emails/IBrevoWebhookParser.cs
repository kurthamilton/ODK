namespace ODK.Services.Emails;

public interface IBrevoWebhookParser
{
    /// <summary>
    /// The event the payload describes, or null where there is nothing to do with it - it belongs to another
    /// deployment, or it cannot be read.
    /// </summary>
    Task<BrevoWebhookEvent?> ParseWebhook(string json);
}
