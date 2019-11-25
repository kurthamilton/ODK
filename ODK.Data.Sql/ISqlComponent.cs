namespace ODK.Data.Sql
{
    public interface ISqlComponent
    {
        string ToSql(SqlContext context);
    }
}
