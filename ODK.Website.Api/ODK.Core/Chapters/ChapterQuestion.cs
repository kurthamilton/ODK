using System;
using System.Collections.Generic;
using System.Text;

namespace ODK.Core.Chapters
{
    public class ChapterQuestion
    {
        public ChapterQuestion(Guid id, Guid chapterId, string name, string answer, int displayOrder)
        {
            Answer = answer;
            ChapterId = chapterId;
            DisplayOrder = displayOrder;
            Id = id;
            Name = name;
        }

        public string Answer { get; }

        public Guid ChapterId { get; }

        public int DisplayOrder { get; }

        public Guid Id { get; }

        public string Name { get; }
    }
}
