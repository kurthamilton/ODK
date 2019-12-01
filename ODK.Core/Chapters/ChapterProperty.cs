using System;
using ODK.Core.DataTypes;

namespace ODK.Core.Chapters
{
    public class ChapterProperty
    {
        public ChapterProperty(Guid id, Guid chapterId, DataType dataType, string name, int displayOrder,
            bool required, string subtitle, string helpText)
        {
            ChapterId = chapterId;
            DataType = dataType;
            DisplayOrder = displayOrder;
            HelpText = helpText;
            Id = id;
            Name = name;
            Required = required;
            Subtitle = subtitle;
        }

        public Guid ChapterId { get; }

        public DataType DataType { get; }

        public int DisplayOrder { get; }

        public string HelpText { get; }

        public Guid Id { get; }

        public string Name { get; }

        public bool Required { get; }

        public string Subtitle { get; }
    }
}
