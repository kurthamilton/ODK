namespace ODK.Core.Chapters;

public class ChapterPropertyOption : IDatabaseEntity
{
    public Guid ChapterPropertyId { get; set; }

    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Value { get; set; } = "";
}
