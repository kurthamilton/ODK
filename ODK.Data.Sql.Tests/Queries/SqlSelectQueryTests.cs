using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;
using ODK.Data.Sql.Tests.Mapping;

namespace ODK.Data.Sql.Tests.Queries
{
    public static class SqlSelectQueryTests
    {
        [Test]
        public static void GetParameterValues_NoConditions_ReturnsEmptyCollection()
        {
            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context);

            (SqlColumn, string, object)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues.Should().BeEmpty();
        }

        [Test]
        public static void GetParameterValues_ReturnsConditionValues()
        {
            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.Int).EqualTo(1)
                .Where(x => x.String).EqualTo("value");

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues
                .Select(x => x.Column.ColumnName)
                .Should()
                .BeEquivalentTo("Int", "String");

            parameterValues
                .Select(x => x.Value)
                .Should()
                .BeEquivalentTo(new object[] { 1, "value" });
        }

        [Test]
        public static void GetParameterValues_ReturnsMultipleConditionValues()
        {
            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.Int).GreaterThan(1)
                .Where(x => x.Int).LessThan(5);

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues
                .Select(x => x.ParameterName)
                .Should()
                .BeEquivalentTo("@Int0", "@Int1");

            parameterValues
                .Select(x => x.Value)
                .Should()
                .BeEquivalentTo(new[] { 1, 5 });
        }

        [Test]
        public static void GetParameterValues_WithWhereAny_ReturnsConditionValues()
        {
            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .WhereAny<TestEntity, int>(x => x.Int, new [] {1, 3, 5});

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues
                .Select(x => x.ParameterName)
                .Should()
                .BeEquivalentTo("@Int0", "@Int1", "@Int2");

            parameterValues
                .Select(x => x.Value)
                .Should()
                .BeEquivalentTo(new[] { 1, 3, 5 });
        }

        [Test]
        public static void GetParameterValues_WithIsNotNull_DoesNotReturnValues()
        {
            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.String).IsNotNull();

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            parameterValues.Should().BeEmpty();
        }

        [Test]
        public static void ToSql_NoConditions_ReturnsSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context);

            string sql = query.ToSql(context);

            sql.Should().Be("SELECT Table.[Int],Table.[String] FROM Table");
        }

        [Test]
        public static void ToSql_WithConditions_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.Int).GreaterThanOrEqualTo(1)
                .Where(x => x.String).EqualTo("abc");

            string sql = query.ToSql(context);

            var expected = "SELECT Table.[Int],Table.[String] FROM Table WHERE (Table.[Int] >= @Int0) AND (Table.[String] = @String0)";
            sql.Should().Be(expected);
        }

        [Test]
        public static void ToSql_WithMultipleConditions_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.Int).GreaterThanOrEqualTo(1)
                .Where(x => x.Int).LessThanOrEqualTo(5);

            string sql = query.ToSql(context);

            var expected = "SELECT Table.[Int],Table.[String] FROM Table WHERE (Table.[Int] >= @Int0) AND (Table.[Int] <= @Int1)";
            sql.Should().Be(expected);
        }

        [Test]
        public static void ToSql_WithConditionalConditions_ReturnsConditionsIfTrue()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .ConditionalWhere(x => x.Int, true).GreaterThanOrEqualTo(1);

            string sql = query.ToSql(context);

            var expected = "SELECT Table.[Int],Table.[String] FROM Table WHERE (Table.[Int] >= @Int0)";
            sql.Should().Be(expected);
        }

        [Test]
        public static void ToSql_WithConditionalConditions_DoesNotReturnConditionsIfFalse()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .ConditionalWhere(x => x.Int, false).GreaterThanOrEqualTo(1);

            string sql = query.ToSql(context);

            sql.Should().Be("SELECT Table.[Int],Table.[String] FROM Table");
        }

        [Test]
        public static void ToSql_WithWhereAnyCondition_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .WhereAny<TestEntity, int>(x => x.Int, new[] {1, 3, 5});

            string sql = query.ToSql(context);

            var expected = "SELECT Table.[Int],Table.[String] FROM Table WHERE (Table.[Int] = @Int0 OR Table.[Int] = @Int1 OR Table.[Int] = @Int2)";
            sql.Should().Be(expected);
        }

        [Test]
        public static void ToSql_WithIsNotNull_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Where(x => x.String).IsNotNull();

            string sql = query.ToSql(context);

            var expected = "SELECT Table.[Int],Table.[String] FROM Table WHERE (Table.[String] IS NOT NULL)";
            sql.Should().Be(expected);
        }

        [Test]
        public static void ToSql_MapsColumnName()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String).HasColumnName("Other");

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context);

            string sql = query.ToSql(context);

            sql.Should().Be("SELECT Table.[Int],Table.[Other] FROM Table");
        }

        [Test]
        public static void ToSql_AddsOrderByFields()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .OrderBy(x => x.Int, SqlSortDirection.Descending)
                .OrderBy(x => x.String);

            string sql = query.ToSql(context);

            sql.Should().Be("SELECT Table.[Int],Table.[String] FROM Table ORDER BY Table.[Int] DESC,Table.[String] ASC");
        }

        [Test]
        public static void ToSql_WithRootTableJoin_ReturnsJoinedTable()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);
            SqlQuery<TestEntity> query = new SqlSelectQuery<TestEntity>(context)
                .Join<TestRelatedEntity, int>(x => x.Int, x => x.Int);

            string sql = query.ToSql(context);

            sql.Should().Be("SELECT Table.[Int],Table.[String] FROM Table JOIN Related ON Table.[Int] = Related.[Int]");
        }

        private static SqlContext CreateMockContext(SqlMap<TestEntity> map = null, SqlMap<TestRelatedEntity> relatedMap = null)
        {
            MockContext context = new MockContext();
            context.AddMockMap(map ?? CreateMap());
            context.AddMockMap(relatedMap ?? CreateRelatedMap());
            return context;
        }

        private static SqlMap<TestEntity> CreateMap()
        {
            TestEntityMap map = new TestEntityMap();
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);
            return map;
        }

        private static SqlMap<TestRelatedEntity> CreateRelatedMap()
        {
            TestRelatedEntityMap map = new TestRelatedEntityMap("Related");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.Other);
            return map;
        }
    }
}
