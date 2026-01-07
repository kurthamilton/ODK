namespace ODK.Core.Chapters;

public class ChapterEventSettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DayOfWeek? DefaultDayOfWeek { get; set; }

    public string? DefaultDescription { get; set; }

    public TimeSpan? DefaultEndTime { get; set; }

    public DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }

    public TimeSpan? DefaultScheduledEmailTimeOfDay { get; set; }

    public TimeSpan? DefaultStartTime { get; set; }

    public bool DisableComments { get; set; }

    public DateTime? GetScheduledDateTime(DateTime? date) => date != null && DefaultScheduledEmailTimeOfDay != null
        ? date + DefaultScheduledEmailTimeOfDay.Value
        : null;
}
