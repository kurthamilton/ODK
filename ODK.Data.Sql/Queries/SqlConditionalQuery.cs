using System.Linq.Expressions;

namespace ODK.Data.Sql.Queries;

public abstract class SqlConditionalQuery<T, TQuery> : SqlQuery<T> where TQuery : SqlConditionalQuery<T, TQuery>
{
    protected SqlConditionalQuery(SqlContext context)
        : base(context)
    {
    }

    protected abstract TQuery Query { get; }

    public SqlQueryCondition<T, T, TValue, TQuery> ConditionalWhere<TValue>(Expression<Func<T, TValue>> expression, bool condition)
    {
        return Where(expression, condition);
    }
    
    public TQuery Join<TTo, TValue>(Expression<Func<T, TValue>> thisField, Expression<Func<TTo, TValue>> toField)
    {
        return Join<T, TTo, TValue>(thisField, toField);
    }

    public TQuery Join<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
    {
        return Join(fromField, toField, SqlJoinType.Inner);
    }

    public TQuery RightJoin<TTo, TValue>(Expression<Func<T, TValue>> thisField, Expression<Func<TTo, TValue>> toField)
    {
        return RightJoin<T, TTo, TValue>(thisField, toField);
    }

    public TQuery RightJoin<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField)
    {
        AddJoin(fromField, toField, SqlJoinType.Right);
        return Query;
    }

    public SqlQueryCondition<T, T, TValue, TQuery> Where<TValue>(Expression<Func<T, TValue>> expression)
    {
        return Where<T, TValue>(expression);
    }

    public SqlQueryCondition<T, TEntity, TValue, TQuery> Where<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression)
    {
        return Where(expression, true);
    }

    public TQuery WhereAny<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, IEnumerable<TValue> values)
    {
        IEnumerable<SqlQueryCondition<T, TEntity, TValue, TQuery>> conditions = values.Select(value =>
        {
            SqlQueryCondition <T, TEntity, TValue, TQuery> condition = new SqlQueryCondition<T, TEntity, TValue, TQuery>(Query, expression);
            condition.EqualTo(value);
            return condition;
        });

        AddConditions(conditions);

        return Query;
    }

    private TQuery Join<TTo, TValue>(Expression<Func<T, TValue>> thisField, Expression<Func<TTo, TValue>> toField, SqlJoinType type)
    {
        return Join<T, TTo, TValue>(thisField, toField, type);
    }

    private TQuery Join<TFrom, TTo, TValue>(Expression<Func<TFrom, TValue>> fromField, Expression<Func<TTo, TValue>> toField, SqlJoinType type)
    {
        AddJoin(fromField, toField, type);
        return Query;
    }

    private SqlQueryCondition<T, TEntity, TValue, TQuery> Where<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression, bool add)
    {
        SqlQueryCondition<T, TEntity, TValue, TQuery> condition = new SqlQueryCondition<T, TEntity, TValue, TQuery>(Query, expression);
        if (add)
        {
            AddCondition(condition);
        }
        return condition;
    }
}
