namespace ODK.Core.Members
{
    public class ChapterModel
    {
        public ChapterModel(int chapterId, string name)
        {
            Id = chapterId;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}
