namespace ODK.Data.Sql.Queries
{
    public class SqlDeleteQuery<T> : SqlConditionalQuery<T>
    {
        public SqlDeleteQuery(SqlContext context)
            : base(context)
        {
            AddDelete();
            AddFrom();
        }
    }
}
