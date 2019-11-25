using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;
using ODK.Data.Sql.Tests.Mapping;

namespace ODK.Data.Sql.Tests.Queries
{
    public static class SqlDeleteQueryTests
    {
        [Test]
        public static void GetParameterValues_ReturnsConditionValues()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlDeleteQuery<TestEntity>(context)
                .Where(x => x.Int).EqualTo(5);

            (SqlColumn Column, object Value)[] parameterValues = query.GetParameterValues().ToArray();

            CollectionAssert.AreEqual(new[] { "Int" }, parameterValues.Select(x => x.Column.ColumnName));
            CollectionAssert.AreEqual(new object[] { 5 }, parameterValues.Select(x => x.Value));
        }

        [Test]
        public static void ToSql_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);

            SqlQuery<TestEntity> query = new SqlDeleteQuery<TestEntity>(context)
                .Where(x => x.Int).EqualTo(5);

            string sql = query.ToSql();

            Assert.AreEqual("DELETE Table FROM Table WHERE Table.[Int] = @Int", sql);
        }

        [Test]
        public static void ToSql_MapsColumnName()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String).HasColumnName("Other");

            SqlContext context = CreateMockContext(map);

            SqlQuery<TestEntity> query = new SqlDeleteQuery<TestEntity>(context)
                .Where(x => x.String).EqualTo("value");

            string sql = query.ToSql();

            Assert.AreEqual("DELETE Table FROM Table WHERE Table.[Other] = @Other", sql);
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
