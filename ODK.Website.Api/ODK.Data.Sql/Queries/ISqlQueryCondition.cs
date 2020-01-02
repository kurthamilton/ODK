namespace ODK.Data.Sql.Queries
{
    public interface ISqlQueryCondition : ISqlComponent
    {
        object Value { get; }
    }
}
