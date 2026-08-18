namespace ODK.Services.Chapters.Workflows;

/// <remarks>Numbered for the reason given on <see cref="ChapterPublicationState"/>.</remarks>
public enum ChapterPublicationTrigger
{
    None = 0,

    /// <summary>A site admin approves the group.</summary>
    Approve = 1,

    /// <summary>The owner publishes it.</summary>
    Publish = 2
}
