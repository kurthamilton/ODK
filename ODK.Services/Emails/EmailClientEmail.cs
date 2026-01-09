using ODK.Core.Emails;

namespace ODK.Services.Emails;

public record EmailClientEmail
{
    public required string Body { get; init; }

    public required EmailAddressee From { get; init; }

    public required DateTime? ScheduledUtc { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyCollection<EmailAddressee> To { get; init; }
}
