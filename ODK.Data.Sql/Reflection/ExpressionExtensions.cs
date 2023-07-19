using System;
using System.Linq.Expressions;

namespace ODK.Data.Sql.Reflection
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }

            throw new ArgumentException("expression");
        }
    }
}
