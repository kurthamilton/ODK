using System;
using ODK.Core.Members;
using ODK.Core.Utils;

namespace ODK.Core.Events;

public class Event : IDatabaseEntity, IChapterEntity
{
    public int? AttendeeLimit { get; set; }

    public bool CanComment => !IsPublic;

    public Guid ChapterId { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public TimeSpan? EndTime { get; set; }

    public Guid Id { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublic { get; set; }

    public bool IsPublished => PublishedUtc != null;

    public string Name { get; set; } = "";

    public DateTime? PublishedUtc { get; set; }

    public bool RsvpDeadlinePassed => RsvpDeadlineUtc < DateTime.UtcNow;

    public DateTime? RsvpDeadlineUtc { get; set; }

    public bool Ticketed => TicketSettings?.Cost > 0;

    public EventTicketSettings? TicketSettings { get; set; }

    public string? Time { get; set; }

    public Guid VenueId { get; set; }

    public static DateTime FromLocalTime(DateTime local, TimeZoneInfo? timeZone)
    {
        if (local.TimeOfDay.TotalSeconds == 0)
        {
            return local;
        }

        return timeZone != null
            ? TimeZoneInfo.ConvertTimeToUtc(local, timeZone)
            : local.SpecifyKind(DateTimeKind.Utc);
    }

    public static DateTime ToLocalTime(DateTime utc, TimeZoneInfo? timeZone)
    {
        if (utc.TimeOfDay.TotalSeconds == 0)
        {
            return utc;
        }

        return timeZone != null
            ? TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone)
            : utc;
    }

    public static string ToLocalTimeString(DateTime utc, TimeSpan? endTime, TimeZoneInfo? timeZone)
    {
        if (utc.TimeOfDay.TotalSeconds == 0)
        {
            return "";
        }

        var localTime = ToLocalTime(utc, timeZone);
        var time = TimeSpanUtils.ToString(localTime.TimeOfDay);
        return endTime != null
            ? $"{time} - {TimeSpanUtils.ToString(endTime)}"
            : $"From {time}";
    }

    public string GetDisplayName() => (!IsPublished ? "[DRAFT] " : "") + Name;    

    public bool IsAuthorized(Member? member)
    {
        if (IsPublic)
        {
            return true;
        }

        return member?.SuperAdmin == true || 
            (member?.IsCurrent() == true && member?.IsMemberOf(ChapterId) == true);
    }

    public DateTime ToLocalTime(TimeZoneInfo? timeZone) => ToLocalTime(Date, timeZone);

    public string ToLocalTimeString(TimeZoneInfo? timeZone) => ToLocalTimeString(Date, EndTime, timeZone);
}
