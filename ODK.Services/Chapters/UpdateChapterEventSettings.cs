namespace ODK.Services.Chapters;

public class UpdateChapterEventSettings
{
    public required DayOfWeek? DefaultDayOfWeek { get; init; }

    public required string? DefaultDescription { get; init; }

    public required DayOfWeek? DefaultScheduledEmailDayOfWeek { get; init; }

    public required TimeSpan? DefaultScheduledEmailTimeOfDay { get; init; }

    public required TimeSpan? DefaultStartTime { get; init; }

    public required bool DisableComments { get; init; }
}
