namespace ODK.Data.Sql.Queries;

public interface ISqlQueryCondition : ISqlComponent
{
    object? GetValue(string parameter);
}
