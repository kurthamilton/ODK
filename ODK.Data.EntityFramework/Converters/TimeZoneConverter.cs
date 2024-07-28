using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class TimeZoneConverter : ValueConverter<TimeZoneInfo?, string?>
{
    public TimeZoneConverter()
        : base(
            x => x != null ? x.Id : null,
            x => x != null ? TimeZoneInfo.FindSystemTimeZoneById(x) : null)
    {
    }
}
