namespace ODK.Services.Emails;

/// <summary>
/// An email with every placeholder resolved and the layout wrapped around it: what a send queues, and what
/// a preview displays.
/// </summary>
public class RenderedEmail
{
    public required string Body { get; init; }

    public required string FromEmailAddress { get; init; }

    public required string FromName { get; init; }

    public required string Subject { get; init; }
}
