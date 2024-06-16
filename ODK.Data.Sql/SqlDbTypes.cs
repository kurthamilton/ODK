using System.Data;

namespace ODK.Data.Sql;

public static class SqlDbTypes
{
    private static readonly IDictionary<Type, SqlDbType> Types = new Dictionary<Type, SqlDbType>
    {
        { typeof(bool), SqlDbType.Bit },
        { typeof(byte), SqlDbType.SmallInt },
        { typeof(byte[]), SqlDbType.VarBinary },
        { typeof(char[]), SqlDbType.NVarChar },
        { typeof(DateTime), SqlDbType.DateTime },
        { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
        { typeof(decimal), SqlDbType.Decimal },
        { typeof(double), SqlDbType.Float },
        { typeof(Guid), SqlDbType.UniqueIdentifier },
        { typeof(int), SqlDbType.Int },
        { typeof(long), SqlDbType.BigInt },
        { typeof(string), SqlDbType.NVarChar }
    };

    public static SqlDbType GetSqlDbType<T>()
    {
        return GetSqlDbType(typeof(T));
    }

    public static SqlDbType GetSqlDbType(Type type)
    {
        if (type.IsEnum)
        {
            type = type.GetEnumUnderlyingType();
        }

        if (Types.ContainsKey(type))
        {
            return Types[type];
        }

        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            return GetSqlDbType(nullableType);
        }

        throw new ArgumentException($"SqlDbType not found for C# type {type.Name}", nameof(type));
    }
}
