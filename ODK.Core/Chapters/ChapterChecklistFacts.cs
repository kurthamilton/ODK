namespace ODK.Core.Chapters;

/// <summary>
/// What the rest of the app already knows about a group, reduced to the questions the checklist asks of
/// it. The caller loads these alongside everything else the page needs, so resolving a checklist is a
/// calculation rather than a second trip to the database.
/// </summary>
/// <remarks>
/// Two steps are absent on purpose. Membership and privacy settings have defaults that are right for most
/// groups, so a group that read them and agreed looks exactly like one that never opened the page - there
/// is nothing here to ask. They are complete only once a recorded row says an organiser looked.
/// </remarks>
public class ChapterChecklistFacts
{
    /// <summary>
    /// When the group's first event was created, which is the timestamp that step is entitled to rather
    /// than whenever the checklist got round to noticing. Null where the group has no events.
    /// </summary>
    public required DateTime? FirstEventCreatedUtc { get; init; }

    /// <summary>
    /// Whether the group has the longer introduction its page opens with.
    /// </summary>
    public required bool HasDescription { get; init; }

    public required bool HasImage { get; init; }

    public required bool HasMemberProperties { get; init; }

    public required bool HasQuestions { get; init; }

    /// <summary>
    /// Whether the group has the line it is listed by. Read alongside <see cref="HasDescription"/>: the
    /// form requires both, so a group has described itself only once it has written both.
    /// </summary>
    public required bool HasShortDescription { get; init; }

    public required bool HasTopics { get; init; }
}
