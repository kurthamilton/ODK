using System;

namespace ODK.Core.Chapters
{
    public class ChapterProperty
    {
        public ChapterProperty(Guid id, Guid chapterId, Guid dataTypeId, string name, int displayOrder, 
            bool required, string subtitle, string helpText)
        {
            ChapterId = chapterId;
            DataTypeId = dataTypeId;
            DisplayOrder = displayOrder;
            HelpText = helpText;
            Id = id;
            Name = name;
            Required = required;
            Subtitle = subtitle;
        }        

        public Guid ChapterId { get; }

        public Guid DataTypeId { get; }

        public int DisplayOrder { get; }

        public string HelpText { get; }

        public Guid Id { get; }

        public string Name { get; }

        public bool Required { get; }

        public string Subtitle { get; }
    }
}
