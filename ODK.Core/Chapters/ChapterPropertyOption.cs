using System;

namespace ODK.Core.Chapters
{
    public class ChapterPropertyOption
    {
        public ChapterPropertyOption(Guid id, Guid chapterPropertyId, Guid chapterId, int displayOrder, string value)
        {
            ChapterId = chapterId;
            ChapterPropertyId = chapterPropertyId;
            DisplayOrder = displayOrder;
            Id = id;
            Value = value;
        }

        public Guid ChapterId { get; }

        public Guid ChapterPropertyId { get; }

        public int DisplayOrder { get; }

        public Guid Id { get; }

        public string Value { get; }
    }
}
