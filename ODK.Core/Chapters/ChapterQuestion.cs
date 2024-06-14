namespace ODK.Core.Chapters;

public class ChapterQuestion : IVersioned
{
    public ChapterQuestion(Guid id, Guid chapterId, string name, string answer, int displayOrder, long version)
    {
        Answer = answer;
        ChapterId = chapterId;
        DisplayOrder = displayOrder;
        Id = id;
        Name = name;
        Version = version;
    }

    public string Answer { get; set; }

    public Guid ChapterId { get; }

    public int DisplayOrder { get; set; }

    public Guid Id { get; }

    public string Name { get; set; }

    public long Version { get; }
}
