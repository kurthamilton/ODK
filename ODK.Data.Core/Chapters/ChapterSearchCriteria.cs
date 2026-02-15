namespace ODK.Data.Core.Chapters;

public class ChapterSearchCriteria
{
    public ChapterSearchCriteriaDistance? Distance { get; init; }

    public IReadOnlyCollection<string>? TopicGroupNames { get; init; }
}