namespace ODK.Data.Sql.Queries
{
    public class SqlDeleteQuery<T> : SqlConditionalQuery<T, SqlDeleteQuery<T>>
    {
        public SqlDeleteQuery(SqlContext context)
            : base(context)
        {
            AddDelete();
        }

        protected override SqlDeleteQuery<T> Query => this;
    }
}
