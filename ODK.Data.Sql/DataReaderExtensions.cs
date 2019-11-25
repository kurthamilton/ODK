using System;
using System.Data;

namespace ODK.Data.Sql
{
    public static class DataReaderExtensions
    {
        public static bool GetBooleanOrDefault(this IDataReader reader, string name)
        {
            return reader.GetValueOrDefault(name, (r, i) => r.GetBoolean(i));
        }

        public static Guid GetGuidOrDefault(this IDataReader reader, string name)
        {
            return reader.GetValueOrDefault(name, (r, i) => r.GetGuid(i));
        }

        public static int GetInt32OrDefault(this IDataReader reader, string name)
        {
            return reader.GetValueOrDefault(name, (r, i) => r.GetInt32(i));
        }

        public static DateTime GetDateTimeOrDefault(this IDataReader reader, string name)
        {
            return reader.GetValueOrDefault(name, (r, i) => r.GetDateTime(i));
        }

        public static string GetStringOrDefault(this IDataReader reader, int ordinal)
        {
            return reader.GetValueOrDefault(ordinal, (r, i) => r.GetString(i));
        }

        public static string GetStringOrDefault(this IDataReader reader, string name)
        {
            return reader.GetValueOrDefault(name, (r, i) => r.GetString(i));
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, string name, Func<IDataReader, int, T> getValue)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.GetValueOrDefault(ordinal, getValue);
        }

        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, Func<IDataReader, int, T> getValue)
        {
            if (reader[ordinal] == DBNull.Value)
            {
                return default(T);
            }

            return getValue(reader, ordinal);
        }
    }
}
