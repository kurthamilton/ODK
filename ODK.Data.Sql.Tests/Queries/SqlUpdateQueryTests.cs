using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;
using ODK.Data.Sql.Tests.Mapping;

namespace ODK.Data.Sql.Tests.Queries
{
    public static class SqlUpdateQueryTests
    {
        [Test]
        public static void GetParameterValues_ReturnsConditionAndUpdateValues()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlUpdateQuery<TestEntity>(context)
                .Set(x => x.String, "updated")
                .Where(x => x.Int).EqualTo(1);

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues
                .Select(x => x.Column.ColumnName)
                .Should()
                .BeEquivalentTo("Int", "String");

            parameterValues
                .Select(x => x.Value)
                .Should()
                .BeEquivalentTo(new object[] { 1, "updated" });
        }

        [Test]
        public static void ToSql_NoConditions_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlUpdateQuery<TestEntity>(context)
                .Set(x => x.Int, 5);

            string sql = query.ToSql(context);

            sql.Should().Be("UPDATE Table SET Table.[Int] = @Int FROM Table");
        }

        [Test]
        public static void ToSql_WithConditions_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlUpdateQuery<TestEntity>(context)
                .Set(x => x.Int, 5)
                .Where(x => x.String).EqualTo("value");

            string sql = query.ToSql(context);

            sql.Should().Be("UPDATE Table SET Table.[Int] = @Int FROM Table WHERE (Table.[String] = @String0)");
        }

        private static SqlContext CreateMockContext(SqlMap<TestEntity> map = null)
        {
            MockContext context = new MockContext();
            context.AddMockMap(map ?? CreateMap());
            return context;
        }

        private static SqlMap<TestEntity> CreateMap()
        {
            TestEntityMap map = new TestEntityMap();
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);
            return map;
        }
    }
}
