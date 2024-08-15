using ODK.Core.DataTypes;

namespace ODK.Core.Chapters;

public class ChapterProperty : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DataType DataType { get; set; }

    public string? DisplayName { get; set; }

    public int DisplayOrder { get; set; }

    public string? HelpText { get; set; }

    public bool ApplicationOnly { get; set; }

    public Guid Id { get; set; }

    public string Label { get; set; } = "";

    public string Name { get; set; } = "";

    public bool Required { get; set; }

    public string? Subtitle { get; set; }
}
