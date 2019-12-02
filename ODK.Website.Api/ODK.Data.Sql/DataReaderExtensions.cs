using System;
using System.Data;

namespace ODK.Data.Sql
{
    public static class DataReaderExtensions
    {
        public static DateTime GetDateTimeOrDefault(this IDataReader reader, int ordinal)
        {
            return reader.GetValueOrDefault(ordinal, (r, i) => r.GetDateTime(i));
        }

        public static string GetStringOrDefault(this IDataReader reader, int ordinal)
        {
            return reader.GetValueOrDefault(ordinal, (r, i) => r.GetString(i));
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, Func<IDataReader, int, T> getValue)
        {
            if (reader[ordinal] == DBNull.Value)
            {
                return default;
            }

            return getValue(reader, ordinal);
        }
    }
}
