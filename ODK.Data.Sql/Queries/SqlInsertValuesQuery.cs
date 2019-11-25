namespace ODK.Data.Sql.Queries
{
    public class SqlInsertValuesQuery<T> : SqlQuery<T>
    {
        public SqlInsertValuesQuery(SqlContext context)
            : base(context)
        {
        }

        public SqlInsertValuesQuery<T> Value(T entity)
        {
            AddInsertColumns(entity);
            return this;
        }
    }
}
