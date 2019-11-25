namespace ODK.Data.Sql.Queries
{
    public interface ISqlQueryCondition : ISqlComponent
    {
        object Value { get; }

        SqlColumn GetColumn(SqlContext context);
    }
}
