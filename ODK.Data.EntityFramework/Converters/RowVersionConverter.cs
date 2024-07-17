using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;
internal class RowVersionConverter : ValueConverter<long, byte[]>
{
    public RowVersionConverter() : base(
        x => BitConverter.GetBytes(x),
        x => BitConverter.ToInt64(x))
    {
    }
}
