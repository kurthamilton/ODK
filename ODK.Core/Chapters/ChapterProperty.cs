using ODK.Core.DataTypes;

namespace ODK.Core.Chapters;

public class ChapterProperty
{
    public ChapterProperty(Guid id, Guid chapterId, DataType dataType, string name, string label, int displayOrder,
        bool required, string? subtitle, string? helpText, bool hidden)
    {
        ChapterId = chapterId;
        DataType = dataType;
        DisplayOrder = displayOrder;
        HelpText = helpText;
        Hidden = hidden;
        Id = id;
        Label = label;
        Name = name;
        Required = required;
        Subtitle = subtitle;
    }

    public Guid ChapterId { get; }

    public DataType DataType { get; set; }

    public int DisplayOrder { get; set; }

    public string? HelpText { get; set; }

    public bool Hidden { get; set; }

    public Guid Id { get; }

    public string Label { get; set; }

    public string Name { get; set; }

    public bool Required { get; set; }

    public string? Subtitle { get; set; }
}
