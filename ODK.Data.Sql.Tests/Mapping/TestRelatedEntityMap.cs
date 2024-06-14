using System;
using System.Data;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Tests.Mapping;

public class TestRelatedEntityMap : SqlMap<TestRelatedEntity>
{
    public TestRelatedEntityMap(string tableName = null)
        : base(tableName ?? "Table")
    {
    }

    public SqlColumn AddProperty<TValue>(Expression<Func<TestRelatedEntity, TValue>> expression)
    {
        return Property(expression);
    }

    public override TestRelatedEntity Read(IDataReader reader)
    {
        throw new NotImplementedException();
    }
}
