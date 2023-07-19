using System;

namespace ODK.Data.Sql.Mapping
{
    public abstract class SqlMap
    {
        protected SqlMap(string tableName, Type type)
        {
            TableName = tableName;
            Type = type;
        }

        public string TableName { get; }

        protected Type Type { get; }
    }
}
