namespace ODK.Core.Chapters;
public class ChapterEventSettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DayOfWeek? DefaultDayOfWeek { get; set; }

    public string? DefaultDescription { get; set; }

    public DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }
    
    public string? DefaultScheduledEmailTimeOfDay { get; set; }    

    public bool DisableComments { get; set; }

    public DateTime? GetScheduledDateTime(DateTime? date)
    {
        if (!TimeOnly.TryParse(DefaultScheduledEmailTimeOfDay, out var time))
        {
            return null;
        }

        return date + time.ToTimeSpan();
    }
}
