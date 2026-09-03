namespace ODK.Data.Core.Events;

/// <summary>
/// What identifies a published event and when it became public, without the event itself. For a caller
/// enumerating every published event on the platform, where loading the entities would be the bulk of the
/// work and none of the answer.
/// </summary>
public class EventPublicationDto
{
    /// <summary>
    /// The chapter the event belongs to. Only meaningful to a caller that asked about several chapters at
    /// once, but carried always so a batched result can be matched back up.
    /// </summary>
    public required Guid ChapterId { get; init; }

    public required DateTime PublishedUtc { get; init; }

    public required string Shortcode { get; init; }
}
