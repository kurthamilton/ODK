using System;
using System.Data;
using System.Linq.Expressions;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Tests.Mapping;

public class TestEntityMap : IEntityTypeConfiguration<TestEntity>
{
    public TestEntityMap(string? tableName = null)
        : base(tableName ?? "Table")
    {
    }

    public SqlColumn AddProperty<TValue>(Expression<Func<TestEntity, TValue>> expression)
    {
        return Property(expression);
    }

    public override TestEntity Read(IDataReader reader)
    {
        throw new NotImplementedException();
    }
}
