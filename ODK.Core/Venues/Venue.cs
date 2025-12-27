namespace ODK.Core.Venues;

public class Venue : IVersioned, IDatabaseEntity, IChapterEntity
{
    public string? Address { get; set; }

    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public string? MapQuery { get; set; }

    public string Name { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}
