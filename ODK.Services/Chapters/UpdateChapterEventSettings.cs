namespace ODK.Services.Chapters;

public class UpdateChapterEventSettings
{
    public required DayOfWeek? DefaultDayOfWeek { get; set; }

    public required string? DefaultDescription { get; set; }

    public required DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }

    public required string? DefaultScheduledEmailTimeOfDay { get; set; }

    public required bool DisableComments { get; set; }
}
