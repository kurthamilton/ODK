namespace ODK.Services.Chapters
{
    public class CreateChapterQuestion
    {
        public CreateChapterQuestion(string? answer, string? name)
        {
            Answer = answer ?? "";
            Name = name ?? "";
        }

        public string Answer { get; }

        public string Name { get; }
    }
}
