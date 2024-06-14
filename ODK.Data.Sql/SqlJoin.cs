using System.Linq.Expressions;

namespace ODK.Data.Sql;

public class SqlJoin<TFrom, TTo, TValue> : ISqlComponent
{
    private readonly Expression<Func<TFrom, TValue>> _fromExpression;
    private readonly Expression<Func<TTo, TValue>> _toExpression;

    public SqlJoin(Expression<Func<TFrom, TValue>> fromExpression, Expression<Func<TTo, TValue>> toExpression, SqlJoinType type = SqlJoinType.Inner)
    {
        _fromExpression = fromExpression;
        _toExpression = toExpression;
        Type = type;
    }

    private SqlJoinType Type { get; }

    public string ToSql(SqlContext context)
    {
        SqlColumn fromColumn = context.GetColumn(_fromExpression);
        SqlColumn toColumn = context.GetColumn(_toExpression);

        return $"{ToSqlJoinType()} JOIN {context.GetTableName<TTo>()} ON {fromColumn.ToSql(context)} = {toColumn.ToSql(context)}";
    }

    private string ToSqlJoinType()
    {
        switch (Type)
        {
            case SqlJoinType.Left:
                return " LEFT";
            case SqlJoinType.Right:
                return " RIGHT";
            default:
                return "";
        }
    }
}
