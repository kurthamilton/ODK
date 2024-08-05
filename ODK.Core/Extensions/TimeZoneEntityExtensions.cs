namespace ODK.Core.Extensions;

public static class TimeZoneEntityExtensions
{
    public static DateTime CurrentTime(this ITimeZoneEntity entity) 
        => entity.ToLocalTime(DateTime.UtcNow);

    public static DateTime FromLocalTime(this ITimeZoneEntity entity, DateTime local) 
        => entity.TimeZone != null ? TimeZoneInfo.ConvertTimeToUtc(local, entity.TimeZone) : local;

    public static DateTime? FromLocalTime(this ITimeZoneEntity entity, DateTime? local) 
        => local != null ? entity.FromLocalTime(local.Value) : null;

    public static DateTime ToLocalTime(this ITimeZoneEntity entity, DateTime utc) 
        => entity.TimeZone != null ? TimeZoneInfo.ConvertTimeFromUtc(utc, entity.TimeZone) : utc;

    public static DateTime? ToLocalTime(this ITimeZoneEntity entity, DateTime? utc) 
        => utc != null ? entity.ToLocalTime(utc.Value) : null;

    public static DateTime TodayUtc(this ITimeZoneEntity entity) 
        => entity.FromLocalTime(entity.CurrentTime().Date);
}
