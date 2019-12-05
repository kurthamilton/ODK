using System;
using System.Data;

namespace ODK.Data.Sql
{
    public static class DataReaderExtensions
    {
        public static DateTime? GetNullableDateTime(this IDataReader reader, int ordinal)
        {
            return reader.GetValueOrDefault(ordinal, (r, i) => r.GetDateTime(i), default(DateTime?));
        }

        public static string GetStringOrDefault(this IDataReader reader, int ordinal)
        {
            return reader.GetValueOrDefault(ordinal, (r, i) => r.GetString(i));
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, Func<IDataReader, int, T> getValue)
        {
            return reader.GetValueOrDefault(ordinal, getValue, default);
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, Func<IDataReader, int, T> getValue, T @default)
        {
            return reader[ordinal] == DBNull.Value ? @default : getValue(reader, ordinal);
        }
    }
}
